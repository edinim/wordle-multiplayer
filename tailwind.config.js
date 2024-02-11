/** @type {import('tailwindcss').Config} */

const colors = require('tailwindcss/colors')

module.exports = {
  content: ["./**/*.{razor,html,cshtml,cs}"],
  theme: {
    extend: {
      colors: {
        ...colors,
      },
      height: {
        screen: ['100vh', '100dvh']
      }
    },
  },
  plugins: [],
}
