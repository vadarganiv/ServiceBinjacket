import type { MetadataRoute } from 'next';

const siteUrl = process.env.PUBLIC_SITE_URL?.trim() || 'http://localhost:3000';

const locales = ['sq', 'en'] as const;
const paths = ['', '/products', '/services', '/repair', '/delivery', '/payment', '/warranty', '/contact'];

export default function sitemap(): MetadataRoute.Sitemap {
  const now = new Date();
  return locales.flatMap(locale =>
    paths.map(path => ({
      url: `${siteUrl}/${locale}${path}`,
      lastModified: now,
      changeFrequency: 'weekly' as const,
      priority: path === '' ? 1.0 : 0.7,
    }))
  );
}
