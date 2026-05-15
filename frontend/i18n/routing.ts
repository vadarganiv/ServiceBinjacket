import { defineRouting } from 'next-intl/routing';

export const routing = defineRouting({
  locales: ['sq', 'en'] as const,
  defaultLocale: 'sq'
});
