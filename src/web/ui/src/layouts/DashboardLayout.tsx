import { useState, type ReactNode } from 'react';

export const DashboardLayout = ({ children }: { children: ReactNode }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);

    return (
        <div className="flex h-screen bg-gray-100 overflow-hidden">
            {/* 1. Menú Lateral (Sidebar) */}
            <aside
                className={`${isCollapsed ? 'w-20' : 'w-64'
                    } bg-slate-800 text-white transition-all duration-300 ease-in-out flex flex-col`}
            >
                <div className="p-4 font-bold text-xl border-b border-slate-700">
                    {isCollapsed ? '🚀' : 'Mi App .NET'}
                </div>
                <nav className="flex-1 p-4 space-y-2">
                    <div className="hover:bg-slate-700 p-2 rounded cursor-pointer">🏠 Home</div>
                    <div className="hover:bg-slate-700 p-2 rounded cursor-pointer">📊 Reports</div>
                </nav>
            </aside>

            {/* Contenedor Derecho (Header + Body + Footer) */}
            <div className="flex-1 flex flex-col relative overflow-hidden">

                {/* 2. Header (Siempre visible) */}
                <header className="h-16 bg-white shadow-md flex items-center justify-between px-6 z-10">
                    <button
                        onClick={() => setIsCollapsed(!isCollapsed)}
                        className="p-2 hover:bg-gray-100 rounded-lg text-gray-600"
                    >
                        {isCollapsed ? '➡️' : '⬅️'}
                    </button>
                    <div className="flex items-center gap-4">
                        <span className="text-sm text-gray-600">Username to change</span>
                        <div className="w-8 h-8 bg-blue-500 rounded-full"></div>
                    </div>
                </header>

                {/* 3. Área de Scroll (Body + Footer) */}
                <main className="flex-1 overflow-y-auto flex flex-col">
                    {/* Contenido Principal */}
                    <div className="flex-1 p-6">
                        {children}
                    </div>

                    {/* 4. Footer (Siempre al final del scroll) */}
                    <footer className="bg-white border-t p-4 text-center text-gray-500 text-sm">
                        © 2026 - Mi Aplicación Aspire - Todos los derechos reservados
                    </footer>
                </main>
            </div>
        </div>
    );
};