'use client';

import { useState } from 'react';
import type { ProductImage } from '@/lib/types';

interface Props {
  images: ProductImage[];
  fallbackImage?: ProductImage | null;
}

export default function ProductImageGallery({ images, fallbackImage }: Props) {
  const allImages = images.length > 0 ? images : fallbackImage ? [fallbackImage] : [];
  const [selectedIndex, setSelectedIndex] = useState(0);
  const selected = allImages[selectedIndex] ?? null;

  return (
    <div className="flex flex-col gap-3">
      <div className="aspect-square bg-muted rounded-2xl overflow-hidden">
        {selected ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={selected.path} alt={selected.alt} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <svg className="w-24 h-24 text-muted-foreground opacity-20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1}
                d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        )}
      </div>

      {allImages.length > 1 && (
        <div className="grid grid-cols-4 gap-2">
          {allImages.slice(0, 4).map((img, i) => (
            <button
              key={i}
              type="button"
              onClick={() => setSelectedIndex(i)}
              className={`aspect-square bg-muted rounded-lg overflow-hidden transition-all ${
                i === selectedIndex
                  ? 'ring-2 ring-primary ring-offset-1'
                  : 'opacity-60 hover:opacity-100'
              }`}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={img.path} alt={img.alt} className="w-full h-full object-cover" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
