import { useState, type ReactNode } from 'react';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { Footer } from './Footer';

export const DashboardLayout = ({ children }: { children: ReactNode }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);

    return (
        <div className="flex h-screen bg-gray-100 overflow-hidden">
            <Sidebar isCollapsed={isCollapsed} />

            <div className="flex-1 flex flex-col relative overflow-hidden">
                <Header onToggle={() => setIsCollapsed(!isCollapsed)} />

                <main className="flex-1 overflow-y-auto flex flex-col">
                    <div className="flex-1 p-6">
                        {children}
                    </div>
                    <Footer />
                </main>
            </div>
        </div>
    );
};