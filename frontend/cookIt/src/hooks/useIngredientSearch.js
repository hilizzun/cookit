import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { API_BASE_URL } from '../config/settings';

export function useIngredientSearch() {
  const { accessToken } = useAuth();
  const [allIngredients, setAllIngredients] = useState([]);
  const [searchResults, setSearchResults] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchAllIngredients = async () => {
      try {
        setIsLoading(true);
        const response = await fetch(`${API_BASE_URL}/api/ingredients`, {
          headers: { Authorization: `Bearer ${accessToken}` }
        });
        
        if (!response.ok) throw new Error('Failed to fetch ingredients');
        
        const data = await response.json();
        setAllIngredients(data);
        setSearchResults(data); 
      } catch (err) {
        setError(err.message);
      } finally {
        setIsLoading(false);
      }
    };

    if (accessToken) {
      fetchAllIngredients();
    }
  }, [accessToken]);

  const searchIngredients = useCallback(async (term) => {
    if (!term.trim()) {
      setSearchResults(allIngredients);
      return;
    }

    try {
      setIsLoading(true);
      const response = await fetch(
        `${API_BASE_URL}/api/ingredients/search?searchText=${encodeURIComponent(term)}`,
        {
          headers: { Authorization: `Bearer ${accessToken}` }
        }
      );
      
      if (!response.ok) throw new Error('Search failed');
      
      const data = await response.json();
      setSearchResults(data);
      setError(null);
    } catch (err) {
      setError(err.message);
      const filtered = allIngredients.filter(ingredient =>
        ingredient.name.toLowerCase().includes(term.toLowerCase())
      );
      setSearchResults(filtered);
    } finally {
      setIsLoading(false);
    }
  }, [accessToken, allIngredients]);

  return {
    allIngredients,
    searchResults,
    searchTerm,
    setSearchTerm,
    searchIngredients,
    isLoading,
    error
  };
}