/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  darkMode: false, // or 'media' or 'class'
  theme: {
    extend: {
      flex: {
        2: '1 0 auto',
      },

      height: {
        '19/20': '95%',
      },

      fontSize: {
        xxs: '0.5rem',
        xxs2: ['9px', '13px'],
        xxs3: ['11px', '20px'],
        sm1: '0.625rem',
        sm2: '0.75rem',
      },

      minHeight: {
        5: '1.25rem',
        '1/4': '15.375rem',
        200: '55.25rem',
        152: '43.25rem',
      },

      maxHeight: {47: '11.75rem', 50: '12.5rem', 128: '31.25rem'},

      maxWidth: {10.375: '10.375rem'},

      colors: {
        gray: {
          10: '#FFFFFF',
          350: '#7e7e7e',
          400: '#6d6d6d',
          450: '#959595',
          500: '#D9D9D9',
          550: '#383838',
          '1C1C1C': '#1c1c1c',
          303030: '#303030',
          252525: '#252525',
        },
        green: {
          300: '#55ffd1',
          500: '#33CA36',
        },
        orange: {
          450: '#FF6200',
          500: '#e57936',
        },
        pink: {
          300: '#FD6C76',
          350: '#C36DC5',
          450: '#9B3E9D',
          500: '#DD4FC1',
        },
        blue: {
          200: '#78949F',
          350: '#63A9E3',
          550: '#417CBF',
        },
        yellow: {
          500: '#FFC11A',
        },
        red: {
          350: '#fc2d3b',
        },
        purple: {
          450: '#8779E2',
        },
      },

      gridTemplateRows: {
        7: '0 230px 288px auto' /* Operational Dashboard Layout */,
      },

      opacity: {
        17: '0.17',
      },
      borderWidth: {
        1: '1px',
      },
    },
  },
  plugins: [],
}
