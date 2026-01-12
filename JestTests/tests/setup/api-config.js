/**
 * Configuration helper for integration tests
 * Determines if API is running and provides connection details
 */

const getApiUrl = () => {
  // Check environment variable first
  if (process.env.API_URL) {
    return process.env.API_URL;
  }
  
  // Default to local development
  return 'http://localhost:5000';
};

const isApiAvailable = async (url) => {
  try {
    const response = await fetch(url, {
      method: 'HEAD',
      timeout: 5000
    });
    return response.ok;
  } catch (error) {
    return false;
  }
};

module.exports = {
  getApiUrl,
  isApiAvailable
};
