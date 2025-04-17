import type { NextConfig } from 'next';
import { createMDX } from 'fumadocs-mdx/next';


/** @type {import('next').NextConfig} */
const config: NextConfig = {
    reactStrictMode: true,
    images: {
        remotePatterns: [
            {
                protocol: 'https',
                hostname: 'avatars.githubusercontent.com',
                port: '',
            },
        ],
    },
};

const withMDX = createMDX();

export default withMDX(config);
