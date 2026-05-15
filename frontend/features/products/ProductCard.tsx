'use client';

import Link from 'next/link';
import { useTranslations } from 'next-intl';
import { useCart } from '@/features/cart/CartContext';
import type { ProductListItem } from '@/lib/types';

const CONDITION_COLORS: Record<string, string> = {
  New: 'bg-emerald-100 text-emerald-800',
  Used: 'bg-amber-100 text-amber-800',
  Refurbished: 'bg-blue-100 text-blue-800',
  Unknown: 'bg-gray-100 text-gray-600',
};

interface Props {
  product: ProductListItem;
  locale: string;
}

export default function ProductCard({ product, locale }: Props) {
  const t = useTranslations('products');
  const tCart = useTranslations('cart');
  const { addItem } = useCart();

  function handleAddToCart(e: React.MouseEvent) {
    e.preventDefault();
    e.stopPropagation();
    addItem({
      productId: product.id,
      slug: product.slug,
      name: product.name,
      price: product.price,
      currency: product.currency,
      image: product.image,
      quantity: 1,
    });
  }

  return (
    <div className="group flex flex-col bg-background border border-border rounded-xl overflow-hidden hover:shadow-lg hover:-translate-y-0.5 transition-all duration-200">
      <Link href={`/${locale}/products/${product.slug}`} className="block">
        <div className="relative aspect-square bg-muted overflow-hidden">
          {product.image ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={product.image.path}
              alt={product.image.alt}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
            />
          ) : (
            <div className="w-full h-full flex items-center justify-center">
              <svg className="w-14 h-14 text-muted-foreground opacity-20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                  d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
          )}
          <span className={`absolute top-2 left-2 text-xs font-medium px-2 py-0.5 rounded-full ${
            CONDITION_COLORS[product.condition] ?? CONDITION_COLORS.Unknown
          }`}>
            {t(`conditionBadge.${product.condition}` as 'conditionBadge.New')}
          </span>
        </div>

        <div className="px-4 pt-4 pb-2">
          <h3 className="text-sm font-semibold leading-tight line-clamp-2 group-hover:text-primary transition-colors">
            {product.name}
          </h3>
          {product.shortDescription && (
            <p className="text-xs text-muted-foreground line-clamp-2 mt-1">{product.shortDescription}</p>
          )}
        </div>
      </Link>

      <div className="px-4 pb-4 pt-1 mt-auto flex items-center justify-between gap-2">
        <div className="flex flex-col">
          <span className="text-base font-bold text-primary whitespace-nowrap">
            {product.price.toLocaleString()} {t('currency')}
          </span>
          <span className={`text-xs font-medium whitespace-nowrap ${
            product.inStock ? 'text-emerald-600' : 'text-red-500'
          }`}>
            {product.inStock ? t('inStock') : t('outOfStock')}
          </span>
        </div>
        <button
          onClick={handleAddToCart}
          disabled={!product.inStock}
          className="flex-shrink-0 flex items-center gap-1.5 px-3 py-2 text-xs font-semibold rounded-xl bg-primary text-primary-foreground hover:bg-primary/90 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >
          <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
              d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          {tCart('addToCart')}
        </button>
      </div>
    </div>
  );
}
