/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./wwwroot/index.html",
        "./Layout/**/*.{razor,html,cshtml}",
        "./Pages/**/*.{razor,html,cshtml}",
    ],
    theme: {
        extend: {
            keyframes: {
                slideIn: {
                    '0%': { transform: 'translateY(-20px)', opacity: '0' },
                    '100%': { transform: 'translateY(0)', opacity: '1' },
                },
                fadeOut: {
                    '0%': { opacity: '1' },
                    '100%': { opacity: '0' },
                },
                progress: {
                    '0%': { width: '100%' },
                    '100%': { width: '0%' },
                },
                tada: {
                    '0%': { transform: 'scale(1)' },
                    '10%, 20%': { transform: 'scale(0.9) rotate(-3deg)' },
                    '30%, 50%, 70%, 90%': { transform: 'scale(1.1) rotate(3deg)' },
                    '40%, 60%, 80%': { transform: 'scale(1.1) rotate(-3deg)' },
                    '100%': { transform: 'scale(1) rotate(0deg)' },
                },
            },
            animation: {
                slideIn: 'slideIn 0.3s ease-out',
                fadeOut: 'fadeOut 0.5s forwards',
                progress: 'progress linear forwards',
                tada: 'tada 0.9s ease-in-out',
            },
        }
    },
    plugins: [
    ],
}
