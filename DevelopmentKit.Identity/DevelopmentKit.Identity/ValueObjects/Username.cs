﻿using System;
using System.ComponentModel;
using System.Globalization;
using Enmeshed.StronglyTypedIds;

namespace Enmeshed.DevelopmentKit.Identity.ValueObjects
{
    [Serializable]
    [TypeConverter(typeof(DeviceNameTypeConverter))]
    public class Username : StronglyTypedId
    {
        public const int MAX_LENGTH = DEFAULT_MAX_LENGTH;

        private const string PREFIX = "USR";

        private static readonly StronglyTypedIdHelpers Utils = new(PREFIX, DefaultValidChars, MAX_LENGTH);

        private Username(string stringValue) : base(stringValue)
        {
        }

        public static Username Parse(string stringValue)
        {
            Utils.Validate(stringValue);

            return new Username(stringValue);
        }

        public static bool IsValid(string stringValue)
        {
            return Utils.IsValid(stringValue);
        }

        public static Username New()
        {
            var deviceNameAsString = StringUtils.Generate(DefaultValidChars, DEFAULT_MAX_LENGTH_WITHOUT_PREFIX);
            return new Username(PREFIX + deviceNameAsString);
        }

        public class DeviceNameTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var stringValue = value as string;

                return !string.IsNullOrEmpty(stringValue)
                    ? Parse(stringValue)
                    : base.ConvertFrom(context, culture, value)!;
            }
        }
    }
}