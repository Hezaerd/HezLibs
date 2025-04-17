import type { Metadata } from "next";

export const baseUrl =
    process.env.NODE_END === 'development'
    ? new URL('http://localhost:3000')
    : new URL('https://hezlibs.hezaerd.com')

export function createMetadata(override: Metadata): Metadata {
    return {
        ...override,
        openGraph: {
          title: override.title ?? undefined,
          description: override.description ?? undefined,
          url: 'https://hezlibs.hezaerd.com',
          images: '/banner.png',
          siteName: 'HezLibs - Documentation',
          ...override.openGraph,
        },
        twitter: {
          card: 'summary_large_image',
          creator: '@hezaerd2',
          title: override.title ?? undefined,
          description: override.description ?? undefined,
          images: '/banner.png',
          ...override.twitter,
        },
      };
}