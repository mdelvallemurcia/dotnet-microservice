export const Header = ({ onToggle }: { onToggle: () => void }) => (
    <header className="h-16 bg-white shadow-md flex items-center justify-between px-6 z-10">
        <button onClick={onToggle} className="p-2 hover:bg-gray-100 rounded-lg text-gray-600">
            ☰
        </button>
        <div className="flex items-center gap-4">
            <span className="text-sm text-gray-600">Usuario Demo</span>
            <div className="w-8 h-8 bg-blue-500 rounded-full"></div>
        </div>
    </header>
);