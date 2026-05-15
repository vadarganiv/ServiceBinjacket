'use client';

import { useTranslations } from 'next-intl';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import { useState } from 'react';

const locales = ['sq', 'en'] as const;
type Locale = typeof locales[number];

export default function Header() {
  const t = useTranslations('nav');
  const router = useRouter();
  const pathname = usePathname();
  const [menuOpen, setMenuOpen] = useState(false);

  const currentLocale = (locales.find(l => pathname.startsWith(`/${l}`)) ?? 'sq') as Locale;

  function switchLocale(newLocale: Locale) {
    const newPath = pathname.replace(`/${currentLocale}`, `/${newLocale}`);
    router.push(newPath);
  }

  const navLinks = [
    { href: `/${currentLocale}`, label: t('home') },
    { href: `/${currentLocale}/products`, label: t('products') },
    { href: `/${currentLocale}/services`, label: t('services') },
    { href: `/${currentLocale}/repair`, label: t('repair') },
  ];

  return (
    <header className="border-b bg-background sticky top-0 z-50">
      <div className="container mx-auto px-4">
        <div className="flex h-16 items-center justify-between">
          {/* Brand */}
          <Link href={`/${currentLocale}`} className="text-xl font-bold text-primary">
            {t('brand')}
          </Link>

          {/* Desktop nav */}
          <nav className="hidden md:flex items-center gap-6">
            {navLinks.map(link => (
              <Link
                key={link.href}
                href={link.href}
                className="text-sm font-medium text-foreground/70 hover:text-foreground transition-colors"
              >
                {link.label}
              </Link>
            ))}
          </nav>

          {/* Language switcher + mobile toggle */}
          <div className="flex items-center gap-2">
            {locales.map(locale => (
              <button
                key={locale}
                onClick={() => switchLocale(locale)}
                className={`px-2 py-1 text-sm font-medium rounded uppercase transition-colors ${
                  locale === currentLocale
                    ? 'bg-primary text-primary-foreground'
                    : 'text-foreground/60 hover:text-foreground'
                }`}
              >
                {locale}
              </button>
            ))}

            {/* Mobile hamburger */}
            <button
              className="md:hidden ml-2 p-2 rounded"
              onClick={() => setMenuOpen(!menuOpen)}
              aria-label={menuOpen ? t('closeMenu') : t('openMenu')}
            >
              <div className="w-5 h-0.5 bg-foreground mb-1" />
              <div className="w-5 h-0.5 bg-foreground mb-1" />
              <div className="w-5 h-0.5 bg-foreground" />
            </button>
          </div>
        </div>

        {/* Mobile menu */}
        {menuOpen && (
          <nav className="md:hidden py-4 border-t flex flex-col gap-3">
            {navLinks.map(link => (
              <Link
                key={link.href}
                href={link.href}
                className="text-sm font-medium text-foreground/70 hover:text-foreground"
                onClick={() => setMenuOpen(false)}
              >
                {link.label}
              </Link>
            ))}
          </nav>
        )}
      </div>
    </header>
  );
}
