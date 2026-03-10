using AppTiemposV3.Api.Data;
using AppTiemposV3.Api.Entities;
using AppTiemposV3.Api.Services.Interfaces;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs.Audits;
using AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.SharedClases.Exceptions;
using AppTiemposV3.SharedClases.Utilidades.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Mysqlx.Cursor;
using static AppTiemposV3.Api.Helpers.DatabaseHelper;
using static AppTiemposV3.Api.Helpers.Helpers;
using static AppTiemposV3.Api.Helpers.MetadataHelper;
using static AppTiemposV3.Api.Utilidades.ConvertByteToFormFile;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static System.Net.HttpStatusCode;

namespace AppTiemposV3.Api.Repositories
{
    public class RequerimentAttachmentRepository : IRequerimentAttachmentContract<RequerimentsAttachmentsDto>
    {
        private readonly AppDbContext _dbCxt;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IUserContract _userContext;
        private readonly IEntityIdProvider _entityIdProvider;
        private readonly IAuditHelperService _auditHelperService;
        private readonly IAlmacenadorArchivos _archiveService;
        private readonly string _contenedor = "requeriments_attachments";
        private Guid _userId => _userContext.GetUserId(); 

        public RequerimentAttachmentRepository(AppDbContext dbCxt, UserManager<UserEntity> userManager, IUserContract userContract, IEntityIdProvider entityIdProvider, IAuditHelperService auditHelperService, IAlmacenadorArchivos archiveService)
        {
            _dbCxt = dbCxt;
            _userManager = userManager;
            _userContext = userContract;
            _entityIdProvider = entityIdProvider;
            _auditHelperService = auditHelperService;
            _archiveService = archiveService;
        }

        public async Task<GeneralResponse> CreateRAttachment(CreateOrUpdateRequerimentAttachmentDto dto, byte[] fileBytes, string fileName)
        {
            UserEntity? user = await GetUserByIdAsync(_userId);

            IFormFile? formFile = ConvertByteArrayToIFormFile(fileBytes, fileName, "application/octet-stream");

            string extension = Path.GetExtension(formFile.FileName);
            string nombreArchivo = $"{ObjectId.GenerateNewId()}{extension}";

            string fecha = dto.AttachmentAt.ToString("dd-MM-yyyy");

            string folder = Path.Combine(
                            _contenedor,
                            _userId.ToString(),
                            fecha,
                            dto.RequerimentId.ToString()
                        );
            string? ruta = await _archiveService.Almacenar(folder, formFile, nombreArchivo);

            if (ruta == null)
            {
                return new GeneralResponse(false, "Error al almacenar el archivo.");
            }

            RequerimentAttachmentEntity requerimentAttachment = new RequerimentAttachmentEntity
            {
                Descripcion = dto.Descripcion,
                AttachmentBy = dto.AttachmentBy,
                FileNameOriginal = fileName,
                FileName = nombreArchivo,
                FilePath = ruta,
                AttachmentAt = dto.AttachmentAt,
                Etapa = dto.Etapa,
                RequerimentId = dto.RequerimentId,
                UserId = _userId
            };

            await _dbCxt.RequerimentAttachments.AddAsync(requerimentAttachment);

            await EnsureSavedAsync("Hubo un error al guardar el registro", _dbCxt);

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se subio un nuevo documento para el requerimiento id: {dto.RequerimentId}",
                    AuditAction.Created.ToString(),
                    nameof(RequerimentAttachmentEntity),
                    "requeriments_attachments",
                    BuildChanges(requerimentAttachment),
                    BuildCreateMetadata(_userId, "CREATE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Archivo adjunto creado exitosamente.", new Dictionary<string, string> { { "FilePath", ruta } });
        }

        public async Task<GeneralResponse> DeleteRAttachment(Guid id)
        {
            UserEntity? user = await GetUserByIdAsync(_userId);
            RequerimentAttachmentEntity docReq = await GetRequerimentAttachmentByIdAsync(id);

            if (docReq == null)
                return new GeneralResponse(false, "El archivo no existe.");

            string ruta = docReq.FilePath;
            Guid requerimentId = docReq.RequerimentId;

            _dbCxt.RequerimentAttachments.Remove(docReq);

            await EnsureSavedAsync("Hubo problemas para eliminar el registro de la base de datos", _dbCxt);

            await _archiveService.Borrar(ruta, $"{_contenedor}/{_userId}/{DateTime.Now:dd-MM-yyyy}/{requerimentId}");

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se eliminó físicamente el documento y el archivo para el requerimiento id: {requerimentId}",
                    AuditAction.Deleted.ToString(),
                    nameof(RequerimentAttachmentEntity),
                    "requeriments_attachments",
                    metadata: BuildCreateMetadata(user!.Id, "HARD_DELETE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error en auditoría: {e.Message}");
            }

            return new GeneralResponse(true, "Archivo y registro eliminados con éxito");
        }
        public async Task<DataAResponse<RequerimentsAttachmentsDto>> GetAllRAttachments()
        {
            List<RequerimentAttachmentEntity>? attachmentsDb = await _dbCxt.RequerimentAttachments
                                .Where(r => r.UserId == _userId && !r.IsDeleted)
                                .ToListAsync();

            // 2. Procesamos en memoria para convertir las rutas a Base64
            List<RequerimentsAttachmentsDto> attachments = new List<RequerimentsAttachmentsDto>();

            foreach (RequerimentAttachmentEntity? r in attachmentsDb)
            {
                RequerimentsAttachmentsDto? dto = new RequerimentsAttachmentsDto
                {
                    Id = r.Id,
                    FileName = r.FileName,
                    FileNameOriginal = r.FileNameOriginal,
                    Descripcion = r.Descripcion,
                    Etapa = r.Etapa,
                    AttachmentBy = r.AttachmentBy,
                    AttachmentAt = r.AttachmentAt,
                    RequerimentId = r.RequerimentId,
                    FileBytes = await _archiveService.ObtenerBase64(r.FilePath)
                };
                attachments.Add(dto);
            }

            return new DataAResponse<RequerimentsAttachmentsDto>(true, attachments, OK);
        }

        public async Task<DataAResponse<RequerimentsAttachmentsDto>> GetAllRAttachmentsByRequerimentId(Guid requerimentId)
        {
            List<RequerimentAttachmentEntity>? attachmentsDb = await _dbCxt.RequerimentAttachments
                                .Where(r => r.UserId == _userId && !r.IsDeleted && r.RequerimentId == requerimentId)
                                .ToListAsync();

            // 2. Procesamos en memoria para convertir las rutas a Base64
            List<RequerimentsAttachmentsDto> attachments = new List<RequerimentsAttachmentsDto>();

            foreach (RequerimentAttachmentEntity? r in attachmentsDb)
            {
                RequerimentsAttachmentsDto? dto = new RequerimentsAttachmentsDto
                {
                    Id = r.Id,
                    FileName = r.FileName,
                    FileNameOriginal = r.FileNameOriginal,
                    Descripcion = r.Descripcion,
                    Etapa = r.Etapa,
                    AttachmentBy = r.AttachmentBy,
                    AttachmentAt = r.AttachmentAt,
                    RequerimentId = r.RequerimentId,
                    FileBytes = await _archiveService.ObtenerBase64(r.FilePath)
                };
                attachments.Add(dto);
            }

            return new DataAResponse<RequerimentsAttachmentsDto>(true, attachments, OK);
        }

        public async Task<DataResponse<RequerimentsAttachmentsDto>> GetRAttachmentById(Guid id)
        {
            RequerimentAttachmentEntity? attachmentsDb = await _dbCxt.RequerimentAttachments
                                 .Where(r => r.UserId == _userId && !r.IsDeleted && r.Id == id)
                                 .FirstOrDefaultAsync();

            
            RequerimentsAttachmentsDto? attachments = new RequerimentsAttachmentsDto
            {
                Id = attachmentsDb!.Id,
                FileName = attachmentsDb.FileName,
                FileNameOriginal = attachmentsDb.FileNameOriginal,
                Descripcion = attachmentsDb.Descripcion,
                Etapa = attachmentsDb.Etapa,
                AttachmentBy = attachmentsDb.AttachmentBy,
                AttachmentAt = attachmentsDb.AttachmentAt,
                RequerimentId = attachmentsDb.RequerimentId,
                FileBytes = await _archiveService.ObtenerBase64(attachmentsDb.FilePath)
            };
            

            return new DataResponse<RequerimentsAttachmentsDto>(true, attachments!, OK);
        }

        public async Task<GeneralResponse> UpdateRAttachment(Guid id, CreateOrUpdateRequerimentAttachmentDto dto, byte[] fileBytes, string fileName)
        {
            UserEntity? user = await GetUserByIdAsync(_userId);
            RequerimentAttachmentEntity oldReq = await GetRequerimentAttachmentByIdAsync(id);
            RequerimentAttachmentEntity docReq = await GetRequerimentAttachmentByIdAsync(id);

            if (docReq == null)
                return new GeneralResponse(false, "El archivo no existe.");


            string? ruta = string.Empty;
            string nameFile = string.Empty;
            string nombreArchivo = string.Empty;

            if (fileBytes != null && !string.IsNullOrWhiteSpace(fileName))
            {
                IFormFile? formFile = ConvertByteArrayToIFormFile(fileBytes, fileName, "application/octet-stream");

                string extension = Path.GetExtension(formFile.FileName);
                nombreArchivo = $"{ObjectId.GenerateNewId()}{extension}";

                string fecha = dto.AttachmentAt.ToString("dd-MM-yyyy");

                string folder = Path.Combine(
                                _contenedor,
                                _userId.ToString(),
                                fecha,
                                dto.RequerimentId.ToString()
                            );

                ruta = await _archiveService.Editar(docReq.FilePath, folder, formFile, nombreArchivo);
                nameFile = fileName;
            }
            else
            {
                ruta = docReq.FilePath;
                nameFile = docReq.FileNameOriginal;
            }

            docReq.Descripcion = dto.Descripcion;
            docReq.AttachmentBy = dto.AttachmentBy;
            docReq.FileName = nombreArchivo ?? docReq.FileName;
            docReq.FileNameOriginal = nameFile;
            docReq.FilePath = ruta;
            docReq.AttachmentAt = dto.AttachmentAt;
            docReq.Etapa = dto.Etapa;
            docReq.RequerimentId = dto.RequerimentId;
            docReq.UserId = _userId;
            docReq.ModifiedAt = DateTime.Now;

            _dbCxt.Entry(docReq).State = EntityState.Modified;

            await EnsureSavedAsync("Hubo un error al actualizar el documento. Intente mas tarde", _dbCxt);

            try
            {
                await _auditHelperService.CreateAuditAsync(
                    user!.FullName,
                    $"Se edito el documento para el requerimiento id: {oldReq.RequerimentId}",
                    AuditAction.Updated.ToString(),
                    nameof(RequerimentAttachmentEntity),
                    "requeriments_attachments",
                    BuildChanges(docReq, oldReq, dto),
                    BuildCreateMetadata(user.Id, "UPDATE")
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new GeneralResponse(true, "Se actualizo con exito el documento.");
        }


        /// <summary>
        /// Obtiene un usuario específico por su Id.
        /// </summary>
        /// <param name="userId">El Id del usuario a buscar.</param>
        /// <returns>Retorna el <see cref="UserEntity"/> correspondiente.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el usuario.</exception>
        private async Task<UserEntity> GetUserByIdAsync(Guid userId)
        {
            UserEntity? user = await _userManager.FindByIdAsync(userId.ToString());
            return user ?? throw new NotFoundException("El usuario no fue encontrado");
        }


        /// <summary>
        /// Obtiene un documento para un requerimiento específico de un usuario, incluyendo su requerimientos y usuarios.
        /// </summary>
        /// <param name="id">El Id del documento a buscar.</param>
        /// <returns>Retorna el <see cref="RequerimentAttachmentEntity"/> correspondiente.</returns>
        /// <exception cref="NotFoundException">Se lanza si no se encuentra el documento del requerimiento.</exception>
        private async Task<RequerimentAttachmentEntity> GetRequerimentAttachmentByIdAsync(Guid id)
        {
            RequerimentAttachmentEntity? requerimentAt = await _dbCxt.RequerimentAttachments
                .Include(r => r.Requeriments)
                .Include(u => u.User)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == _userId && !r.IsDeleted);

            return requerimentAt ?? throw new NotFoundException("Documento del Requerimiento no encontrado");
        }


        private static List<AuditChangeDto> BuildChanges(
            RequerimentAttachmentEntity newReq,
            RequerimentAttachmentEntity? oldReq = null,
            CreateOrUpdateRequerimentAttachmentDto? dto = null)
        {
            List<AuditChangeDto> changes = new();

            if (oldReq is null)
            {
                AddCreate(nameof(newReq.Descripcion), newReq.Descripcion);
                AddCreate(nameof(newReq.AttachmentAt), newReq.AttachmentAt);
                AddCreate(nameof(newReq.AttachmentBy), newReq.AttachmentBy);
                AddCreate(nameof(newReq.FileName), newReq.FileName);
                AddCreate(nameof(newReq.FilePath), newReq.FilePath);
                AddCreate(nameof(newReq.Etapa), newReq.Etapa);
                return changes;
            }

            if (dto is not null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Descripcion) &&
                    dto.Descripcion != oldReq.Descripcion)
                    AddUpdate(nameof(newReq.Descripcion), oldReq.Descripcion, newReq.Descripcion);

                if (dto.AttachmentAt != oldReq.AttachmentAt)
                    AddUpdate(nameof(newReq.AttachmentAt), oldReq.AttachmentAt, newReq.AttachmentAt);

                if (!string.IsNullOrWhiteSpace(dto.AttachmentBy) &&
                    dto.AttachmentBy != oldReq.AttachmentBy)
                    AddUpdate(nameof(newReq.AttachmentBy), oldReq.AttachmentBy, newReq.AttachmentBy);             

                if (dto.Etapa != oldReq.Etapa)
                    AddUpdate(nameof(newReq.Etapa), oldReq.Etapa, newReq.Etapa);
            }

            return changes;

            void AddCreate(string field, object? value)
            {
                changes.Add(new AuditChangeDto
                {
                    Field = field,
                    NewValue = NormalizeValue(value?.ToString())
                });
            }

            void AddUpdate(string field, object? oldValue, object? newValue)
            {
                string? oldVal = oldValue?.ToString();
                string? newVal = newValue?.ToString();

                if (oldVal != newVal)
                {
                    changes.Add(new AuditChangeDto
                    {
                        Field = field,
                        OldValue = NormalizeValue(oldValue),
                        NewValue = NormalizeValue(newValue)
                    });
                }
            }
        }
    }
}
