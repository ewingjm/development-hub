module.exports = {
    parser: "@typescript-eslint/parser",
    parserOptions: {
      ecmaVersion: 2015,
      sourceType: "script",
      project: ['tsconfig.json'],
    },
    extends: [
      'airbnb-typescript/base',
    ],
    globals: {
      "Xrm": "readonly",
      "window": "readonly",
      "fetch": "readonly",
    },
    rules: {
      'max-classes-per-file': 'off',
      'no-inner-declarations': 'off',
      'import/prefer-default-export': 'off'
    },
  };