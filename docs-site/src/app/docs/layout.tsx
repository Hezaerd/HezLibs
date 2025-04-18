import { DocsLayout, DocsLayoutProps } from 'fumadocs-ui/layouts/docs';
import type { ReactNode } from 'react';
import { baseOptions } from '@/app/layout.config';
import { source } from '@/lib/source';
import { Group } from 'lucide-react';

const docsOptions: DocsLayoutProps = {
  ...baseOptions,
  tree: source.pageTree,
  links: [
    {
      icon: <Group />,
      text: 'FSM',
      description: 'Powerful finite state machine implementation',
      url: '/docs/fsm',
    },
  ],
};
export default function Layout({ children }: { children: ReactNode }) {
  return <DocsLayout {...docsOptions}> {children} </DocsLayout>
}
