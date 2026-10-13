import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      globals: globals.browser,
    },
    rules: {
      // Code outside a feature imports it through its index.ts, never its internal files.
      'no-restricted-imports': [
        'error',
        {
          patterns: [
            {
              group: ['**/features/*/*'],
              message: 'Import from the feature folder (its index.ts), not from files inside it.',
            },
          ],
        },
      ],
    },
  },
])
