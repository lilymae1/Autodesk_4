import pkg from 'eslint';
const { defineConfig } = pkg;

export default defineConfig({
  extends: ['eslint:recommended'],
  parserOptions: {
    ecmaVersion: 2020,
    sourceType: 'module',
  },
  env: {
    browser: true,
    node: true,
  },
});