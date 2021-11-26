﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Xunit.Sdk;

namespace Enmeshed.UnitTestTools.FluentValidation
{
    public static class ValidationFailuresExtensions
    {
        public static IEnumerable<ValidationFailure> WithErrorMessagePattern(
            this IEnumerable<ValidationFailure> validationFailures, string pattern)
        {
            var regexString = pattern.Replace("*", ".*");
            return validationFailures.When(failure => failure.ErrorMessage.MatchesRegex(regexString),
                $"Expected an error message of '{pattern}'. Actual message was '{{Message}}'");
        }
    }

    public static class StringExtensions
    {
        public static bool MatchesRegex(this string text, string regexString)
        {
            var regex = new Regex(regexString);
            return regex.IsMatch(text);
        }
    }

    public static class IValidatorExtensions
    {
        public static IEnumerable<ValidationFailure> ShouldHaveValidationErrorForItemWithIndex<T>(
            this IValidator<T> validator, T objectToValidate, int indexOfItem) where T : class
        {
            var validationResult = validator.Validate(objectToValidate);

            var searchedText = $"[{indexOfItem.ToString()}]";

            var propertyNamesOfErrorMessages = validationResult.Errors.Select(r => r.PropertyName);

            if (!propertyNamesOfErrorMessages.Any(m => m.Contains(searchedText)))
                throw new XunitException($"Expected error for item with index {indexOfItem}.");

            return validationResult.Errors;
        }
    }
}