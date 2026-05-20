import { getTranslations } from 'next-intl/server';
import { notFound } from 'next/navigation';
import Link from 'next/link';
import type { Metadata } from 'next';
import { apiFetch } from '@/lib/api';
import type { ProductDetail } from '@/lib/types';
import AddToCartButton from '@/features/products/AddToCartButton';
import ProductImageGallery from '@/features/products/ProductImageGallery';

type Props = {
  params: Promise<{ locale: string; slug: string }>;
};

const CONDITION_COLORS: Record<string, string> = {
  New: 'bg-emerald-100 text-emerald-800',
  Used: 'bg-amber-100 text-amber-800',
  Refurbished: 'bg-blue-100 text-blue-800',
  Unknown: 'bg-gray-100 text-gray-600',
};

async function fetchProduct(slug: string, locale: string): Promise<ProductDetail | null> {
  try {
    return await apiFetch<ProductDetail>(`/products/${slug}?locale=${locale}`);
  } catch {
    return null;
  }
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale, slug } = await params;
  const product = await fetchProduct(slug, locale);
  if (!product) return { title: 'Not found' };

  return {
    title: product.name,
    description: product.shortDescription ?? undefined,
    alternates: {
      canonical: `/${locale}/products/${slug}`,
      languages: {
        sq: `/sq/products/${slug}`,
        en: `/en/products/${slug}`,
        'x-default': `/sq/products/${slug}`,
      },
    },
    openGraph: {
      title: product.name,
      description: product.shortDescription ?? undefined,
      images: product.image ? [product.image.path] : [],
      locale,
      type: 'website',
    },
  };
}

export default async function ProductDetailPage({ params }: Props) {
  const { locale, slug } = await params;

  const [product, t, tProducts] = await Promise.all([
    fetchProduct(slug, locale),
    getTranslations({ locale, namespace: 'productDetails' }),
    getTranslations({ locale, namespace: 'products' }),
  ]);

  if (!product) notFound();

  return (
    <div className="container mx-auto px-4 py-8">
      <Link
        href={`/${locale}/products`}
        className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground mb-6 transition-colors group"
      >
        <svg className="w-4 h-4 group-hover:-translate-x-0.5 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
        </svg>
        {t('backToProducts')}
      </Link>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 lg:gap-12">
        {/* Image */}
        <ProductImageGallery images={product.images ?? []} fallbackImage={product.image} />

        {/* Info */}
        <div className="flex flex-col gap-5">
          <div>
            <div className="flex flex-wrap items-center gap-2 mb-3">
              <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${
                CONDITION_COLORS[product.condition] ?? CONDITION_COLORS.Unknown
              }`}>
                {tProducts(`conditionBadge.${product.condition}` as 'conditionBadge.New')}
              </span>
              <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${
                product.inStock ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-600'
              }`}>
                {product.inStock ? t('inStock') : t('outOfStock')}
              </span>
            </div>
            <h1 className="text-2xl font-bold text-foreground leading-snug">{product.name}</h1>
          </div>

          <div className="text-3xl font-bold text-primary">
            {product.price.toLocaleString()} {tProducts('currency')}
          </div>

          {product.shortDescription && (
            <p className="text-muted-foreground leading-relaxed">{product.shortDescription}</p>
          )}

          <div className="grid grid-cols-2 gap-4 py-4 border-y border-border">
            {product.category && (
              <div>
                <p className="text-xs text-muted-foreground uppercase tracking-wider mb-1">{t('category')}</p>
                <p className="text-sm font-medium text-foreground">{product.category.name}</p>
              </div>
            )}
            {product.warrantyMonths != null && product.warrantyMonths > 0 && (
              <div>
                <p className="text-xs text-muted-foreground uppercase tracking-wider mb-1">{t('warranty')}</p>
                <p className="text-sm font-medium text-foreground">
                  {t('warrantyMonths', { months: product.warrantyMonths })}
                </p>
              </div>
            )}
          </div>

          <AddToCartButton product={product} locale={locale} />
        </div>
      </div>

      {product.description && (
        <div className="mt-12 max-w-3xl">
          <h2 className="text-xl font-semibold mb-4">{t('description')}</h2>
          <p className="text-muted-foreground leading-relaxed whitespace-pre-line">{product.description}</p>
        </div>
      )}
    </div>
  );
}
