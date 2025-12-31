// <copyright file="SecurePasswordGenerator.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Cms.Common.Utilities;

using System;
using System.Linq;
using System.Security.Cryptography;

/// <summary>
/// Utility for generating cryptographically secure passwords and secrets.
/// </summary>
public static class SecurePasswordGenerator
{
    private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string SpecialChars = "!@#$%^&*-_+=";

    /// <summary>
    /// Generates a cryptographically secure random password.
    /// </summary>
    /// <param name="length">The length of the password (minimum 16, default 32).</param>
    /// <param name="includeSpecialChars">Whether to include special characters.</param>
    /// <returns>A secure random password.</returns>
    /// <exception cref="ArgumentException">Thrown when length is less than 16.</exception>
    public static string GeneratePassword(int length = 32, bool includeSpecialChars = true)
    {
        if (length < 16)
        {
            throw new ArgumentException("Password length must be at least 16 characters.", nameof(length));
        }

        var characterSet = UpperCase + LowerCase + Digits;
        if (includeSpecialChars)
        {
            characterSet += SpecialChars;
        }

        var password = new char[length];
        var randomBytes = new byte[length * 4];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (int i = 0; i < length; i++)
        {
            var randomIndex = BitConverter.ToUInt32(randomBytes, i * 4) % (uint)characterSet.Length;
            password[i] = characterSet[(int)randomIndex];
        }

        // Ensure at least one character from each required set
        EnsureComplexity(password, characterSet, includeSpecialChars);

        return new string(password);
    }

    /// <summary>
    /// Generates a URL-safe base64 token (no special characters, safe for URLs).
    /// </summary>
    /// <param name="byteLength">Number of random bytes (default 32 = 256 bits).</param>
    /// <returns>A URL-safe base64 string.</returns>
    public static string GenerateUrlSafeToken(int byteLength = 32)
    {
        var randomBytes = new byte[byteLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", string.Empty);
    }

    /// <summary>
    /// Ensures password has at least one character from each required character set.
    /// </summary>
    private static void EnsureComplexity(char[] password, string characterSet, bool includeSpecialChars)
    {
        var random = new Random(BitConverter.ToInt32(RandomNumberGenerator.GetBytes(4)));

        // Ensure at least one uppercase
        if (!password.Any(c => UpperCase.Contains(c)))
        {
            password[random.Next(password.Length)] = UpperCase[random.Next(UpperCase.Length)];
        }

        // Ensure at least one lowercase
        if (!password.Any(c => LowerCase.Contains(c)))
        {
            password[random.Next(password.Length)] = LowerCase[random.Next(LowerCase.Length)];
        }

        // Ensure at least one digit
        if (!password.Any(c => Digits.Contains(c)))
        {
            password[random.Next(password.Length)] = Digits[random.Next(Digits.Length)];
        }

        // Ensure at least one special char (if required)
        if (includeSpecialChars && !password.Any(c => SpecialChars.Contains(c)))
        {
            password[random.Next(password.Length)] = SpecialChars[random.Next(SpecialChars.Length)];
        }
    }
}