﻿using MCB.Core.Infra.CrossCutting.DesignPatterns.Validator.Abstractions;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Validator.Abstractions.Enums;
using MCB.Core.Infra.CrossCutting.DesignPatterns.Validator.Abstractions.Models;

namespace MCB.Core.Infra.CrossCutting.DesignPatterns.Validator
{
    public abstract class ValidatorBase<T>
        : IValidator<T>
    {
        // Fields
        private bool _hasFluentValidationValidatorWrapperConfigured;
        private readonly FluentValidationValidatorWrapper _fluentValidationValidatorWrapper;

        // Constructors
        protected ValidatorBase()
        {
            _fluentValidationValidatorWrapper = new FluentValidationValidatorWrapper();
        }

        // Private Methods
        private static ValidationMessageType CreateValidationMessageType(FluentValidation.Severity severity)
        {
            ValidationMessageType validationMessageType;

            if (severity == FluentValidation.Severity.Error)
                validationMessageType = ValidationMessageType.Error;
            else if(severity == FluentValidation.Severity.Warning)
                validationMessageType = ValidationMessageType.Warning;
            else
                validationMessageType = ValidationMessageType.Information;

            return validationMessageType;
        }
        private static ValidationResult CreateValidationResult(FluentValidation.Results.ValidationResult fluentValidationValidationResult)
        {
            var validationMessageCollection = new List<ValidationMessage>();

            foreach (var validationFailure in fluentValidationValidationResult.Errors)
                validationMessageCollection.Add(
                    new ValidationMessage(
                        validationMessageType: CreateValidationMessageType(validationFailure.Severity),
                        code: validationFailure.ErrorCode,
                        description: validationFailure.ErrorMessage
                    )
                );

            return new ValidationResult(validationMessageCollection);
        }
        private void CheckAndConfigureFluentValidationConcreteValidator()
        {
            if (_hasFluentValidationValidatorWrapperConfigured)
                return;

            ConfigureFluentValidationConcreteValidator(_fluentValidationValidatorWrapper);

            _hasFluentValidationValidatorWrapperConfigured = true;
        }

        // Protected Methods
        protected abstract void ConfigureFluentValidationConcreteValidator(FluentValidationValidatorWrapper fluentValidationValidatorWrapper);

        // Public Methods
        public ValidationResult Validate(T instance)
        {
            CheckAndConfigureFluentValidationConcreteValidator();

            return CreateValidationResult(_fluentValidationValidatorWrapper.Validate(instance));
        }
        public async Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken)
        {
            CheckAndConfigureFluentValidationConcreteValidator();

            return CreateValidationResult(await _fluentValidationValidatorWrapper.ValidateAsync(instance, cancellationToken));
        }

        #region Fluent Validation Wrapper
        public class FluentValidationValidatorWrapper
            : FluentValidation.AbstractValidator<T>
        {

        } 
        #endregion
    }
}
