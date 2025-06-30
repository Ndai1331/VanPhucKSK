const colors = require('tailwindcss/colors');

module.exports = {
    content: [
        "./src/**/*.{html,js}",
        "./src/assets/libs/*"
    ],
    darkMode: ['class', '[data-mode="dark"]'],
    theme: {
        container: {
            center: true,
            padding: '1.5rem',
        },
        extend: {
            fontFamily: {
                cerebri: ["Cerebri Sans", "sans-serif"],
            },
            colors: {
                transperent: "transperent",
                current: "currentColor",
                muted: "#94989a",
                white: "#ffffff",
                light: "#e2e6eb",
                black: "#323a46",
                purple: "#6a69f5",
                success: "#50cd89",
                danger: "#f1416c",
                warning: "#ffc700",
                info: "#009ef7",
                dark: "#151515",
                darklight: "#1F1F1F",
                darkborder: "#343331",
                darkmuted: "#767273",
            },
            dark: {
                50: colors.slate[50],
                100: colors.slate[100],
                200: colors.slate[200],
                300: colors.slate[300],
                400: colors.slate[400],
                500: colors.slate[500],
                600: colors.slate[600],
                700: colors.slate[700],
                800: colors.slate[800],
                850: '#161C30',
                900: colors.slate[900],
                950: colors.slate[950],
            },
        },
    },
    plugins: [
        require("@tailwindcss/forms")({
            strategy: "base", // only generate global styles
        }),
        require("tailwind-scrollbar"),
        require('./wwwroot/plugins/layouts/layouts'),
        require('./wwwroot/plugins/layouts/sidebar'),
        require('./wwwroot/plugins/card'),
        require('./wwwroot/plugins/buttons'),
        require('./wwwroot/plugins/forms'),
        require('./wwwroot/plugins/tables'),
        require('./wwwroot/plugins/plugins/flatpicker'),
        require('./wwwroot/plugins/plugins/apexchart'),
    ],
}