const colors = require('tailwindcss/colors');
module.exports = {
    purge: {
        enabled: true,
        content: [
            './**/*.html',
            './**/*.razor'
        ]
    },
    theme: {
        extend: {}
    },
    plugins: []
}