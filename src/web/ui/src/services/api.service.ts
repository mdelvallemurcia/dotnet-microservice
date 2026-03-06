const API_URL = import.meta.env.VITE_API_URL;

export const authService = {
    login: async (username: string, password: string) => {
        const response = await fetch(`${API_URL}/v1/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ username, password }),
        });

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || 'Auth error');
        }

        return response.json();
    }
};