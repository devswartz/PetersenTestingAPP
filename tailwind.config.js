/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme')
module.exports = {
  content: [
    './PetersenTestingApp/**/*.{razor,html,cshtml}',
    './PetersenTestingApp/Pages/**/*.{razor,html,cshtml}',
    './PetersenTestingApp/Components/**/*.{razor,html,cshtml}',
],
  theme: {
    extend: {},
  },
  plugins: [
require('@tailwindcss/forms'),
require('@tailwindcss/aspect-ratio')

],
}

