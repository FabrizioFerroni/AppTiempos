// const colors = require('tailwindcss/colors');
// module.exports = {
//     darkMode: 'class',
//     purge: {
//         enabled: true,
//         content: [
//             './**/*.html',
//             './**/*.razor'
//         ]
//     },
//     theme: {
//         extend: {}
//     },
//     plugins: []
// }

/** @type {import('tailwindcss').Config} */
module.exports = {
    darkMode: 'class',
    content: [
        "./**/*.razor",
        "./**/*.html",
        "./**/*.cshtml",
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}
