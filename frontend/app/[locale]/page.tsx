import Link from 'next/link';
import { getTranslations } from 'next-intl/server';
import type { Metadata } from 'next';
import { apiFetch } from '@/lib/api';
import type { PagedResult, ProductListItem, ServiceListItem } from '@/lib/types';
import ProductCard from '@/features/products/ProductCard';
import ServiceCard from '@/features/services/ServiceCard';

type Props = {
  params: Promise<{ locale: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'home' });
  return {
    title: t('heroTitle'),
    description: t('heroSubtitle'),
    alternates: {
      canonical: `/${locale}`,
      languages: { sq: '/sq', en: '/en', 'x-default': '/sq' },
    },
    openGraph: {
      title: t('heroTitle'),
      description: t('heroSubtitle'),
      locale,
    },
  };
}

async function fetchFeaturedProducts(locale: string): Promise<ProductListItem[]> {
  try {
    const data = await apiFetch<PagedResult<ProductListItem>>(
      `/products?locale=${locale}&sort=newest&page=1&pageSize=8`
    );
    return data.items;
  } catch {
    return [];
  }
}

async function fetchFeaturedServices(locale: string): Promise<ServiceListItem[]> {
  try {
    const all = await apiFetch<ServiceListItem[]>(`/services?locale=${locale}`);
    return all.slice(0, 6);
  } catch {
    return [];
  }
}

function whatsappLink(phone: string, locale: string) {
  const digits = phone.replace(/\D/g, '');
  const text = encodeURIComponent(
    locale === 'sq'
      ? 'Përshëndetje! Kam një pyetje për Servis Binjaket.'
      : 'Hello! I have a question for Servis Binjaket.'
  );
  return digits ? `https://wa.me/${digits}?text=${text}` : '#';
}

export default async function HomePage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'home' });

  const [products, services] = await Promise.all([
    fetchFeaturedProducts(locale),
    fetchFeaturedServices(locale),
  ]);

  const whatsappPhone = process.env.WHATSAPP_PHONE ?? '';
  const publicPhone = process.env.PUBLIC_PHONE ?? whatsappPhone;
  const waUrl = whatsappLink(whatsappPhone, locale);
  const telUrl = publicPhone ? `tel:${publicPhone.replace(/\s/g, '')}` : '#';

  return (
    <div>
      {/* Hero */}
      <section className="bg-gradient-to-b from-primary/5 to-transparent">
        <div className="container mx-auto px-4 py-16 sm:py-24 text-center">
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold tracking-tight text-foreground">
            {t('heroTitle')}
          </h1>
          <p className="mt-5 max-w-2xl mx-auto text-lg sm:text-xl text-muted-foreground">
            {t('heroSubtitle')}
          </p>
          <div className="mt-8 flex flex-col sm:flex-row gap-3 sm:gap-4 justify-center">
            <Link
              href={`/${locale}/repair`}
              className="inline-flex items-center justify-center px-6 py-3 rounded-xl bg-primary text-primary-foreground font-semibold hover:bg-primary/90 transition-colors"
            >
              {t('ctaRepair')}
            </Link>
            <Link
              href={`/${locale}/products`}
              className="inline-flex items-center justify-center px-6 py-3 rounded-xl border border-border bg-background text-foreground font-semibold hover:bg-muted transition-colors"
            >
              {t('ctaShop')}
            </Link>
          </div>
        </div>
      </section>

      {/* Services */}
      <section className="container mx-auto px-4 py-12 sm:py-16">
        <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3 mb-8">
          <div>
            <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">{t('servicesTitle')}</h2>
            <p className="mt-1 text-muted-foreground">{t('servicesSubtitle')}</p>
          </div>
          <Link
            href={`/${locale}/services`}
            className="text-sm font-medium text-primary hover:underline whitespace-nowrap"
          >
            {t('servicesViewAll')} →
          </Link>
        </div>
        {services.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t('servicesEmpty')}</p>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {services.map(service => (
              <ServiceCard key={service.id} service={service} locale={locale} />
            ))}
          </div>
        )}
      </section>

      {/* Featured products */}
      <section className="bg-muted/30">
        <div className="container mx-auto px-4 py-12 sm:py-16">
          <div className="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-3 mb-8">
            <div>
              <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">{t('productsTitle')}</h2>
              <p className="mt-1 text-muted-foreground">{t('productsSubtitle')}</p>
            </div>
            <Link
              href={`/${locale}/products`}
              className="text-sm font-medium text-primary hover:underline whitespace-nowrap"
            >
              {t('productsViewAll')} →
            </Link>
          </div>
          {products.length === 0 ? (
            <p className="text-sm text-muted-foreground">{t('productsEmpty')}</p>
          ) : (
            <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-3 md:gap-4">
              {products.map(product => (
                <ProductCard key={product.id} product={product} locale={locale} />
              ))}
            </div>
          )}
        </div>
      </section>

      {/* How it works */}
      <section className="container mx-auto px-4 py-12 sm:py-16">
        <div className="max-w-2xl mx-auto text-center mb-10">
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">{t('howItWorksTitle')}</h2>
          <p className="mt-2 text-muted-foreground">{t('howItWorksSubtitle')}</p>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {[1, 2, 3, 4].map(n => (
            <div key={n} className="flex flex-col items-start gap-3">
              <span className="w-10 h-10 rounded-full bg-primary/10 text-primary flex items-center justify-center font-bold">
                {n}
              </span>
              <h3 className="font-semibold text-lg">{t(`step${n}Title` as 'step1Title')}</h3>
              <p className="text-sm text-muted-foreground">{t(`step${n}Desc` as 'step1Desc')}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Contact CTA */}
      <section className="bg-primary/5 border-t border-border">
        <div className="container mx-auto px-4 py-12 sm:py-16 text-center">
          <h2 className="text-2xl sm:text-3xl font-bold tracking-tight">{t('contactTitle')}</h2>
          <p className="mt-2 text-muted-foreground max-w-xl mx-auto">{t('contactSubtitle')}</p>
          <div className="mt-6 flex flex-col sm:flex-row gap-3 sm:gap-4 justify-center">
            <a
              href={waUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl bg-green-500 hover:bg-green-600 text-white font-semibold transition-colors"
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347z" />
                <path d="M11.99 0C5.375 0 0 5.373 0 12c0 2.117.553 4.103 1.518 5.829L0 24l6.335-1.652A11.954 11.954 0 0011.99 24C18.614 24 24 18.627 24 12S18.614 0 11.99 0zm.01 21.818a9.817 9.817 0 01-5.006-1.362l-.36-.214-3.732.979 1.001-3.648-.235-.374A9.818 9.818 0 012.182 12c0-5.42 4.41-9.818 9.818-9.818 5.42 0 9.818 4.41 9.818 9.818 0 5.42-4.398 9.818-9.818 9.818z" />
              </svg>
              {t('contactWhatsApp')}
            </a>
            {publicPhone && (
              <a
                href={telUrl}
                className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-xl border border-border bg-background text-foreground font-semibold hover:bg-muted transition-colors"
              >
                {t('contactCall')}: {publicPhone}
              </a>
            )}
          </div>
        </div>
      </section>
    </div>
  );
}
