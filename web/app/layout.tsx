import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "假面骰子 · 在线游戏",
  description: "深棕色与白色的 1-bit 心理博弈，DeepSeek 生成对手的微表情。",
  other: {
    "codex-preview": "development",
  },
  icons: {
    icon: "/favicon.svg",
    shortcut: "/favicon.svg",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="zh-CN">
      <body className="antialiased">{children}</body>
    </html>
  );
}
