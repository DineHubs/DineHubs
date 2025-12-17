/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Background colors - use with bg-bg-light and dark:bg-bg-dark
        'bg-light': '#FAFAFA',
        'bg-dark': '#0A0A0F',
        // Surface colors - use with bg-surface-light and dark:bg-surface-dark
        'surface-light': '#FFFFFF',
        'surface-dark': '#141420',
        // Text colors - use with text-text-primary-light and dark:text-text-primary-dark
        'text-primary-light': '#111827',
        'text-primary-dark': '#F9FAFB',
        'text-secondary-light': '#6B7280',
        'text-secondary-dark': '#9CA3AF',
        // Border colors - use with border-border-light and dark:border-border-dark
        'border-light': '#E5E7EB',
        'border-dark': '#1F2937',
        // Semantic colors
        'primary': {
          DEFAULT: '#6366F1',
          light: '#4F46E5',
        },
        'secondary': {
          DEFAULT: '#8B5CF6',
          light: '#7C3AED',
        },
        'success': '#10B981',
        'warning': '#F59E0B',
        'error': '#EF4444',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
      borderRadius: {
        'xl': '1rem',
        '2xl': '1.5rem',
      },
      backdropBlur: {
        'xl': '24px',
      },
    },
  },
  plugins: [],
}

