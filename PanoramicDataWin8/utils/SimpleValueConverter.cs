﻿using System;
using System.Diagnostics.Contracts;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PanoramicDataWin8.utils
{
    public abstract class SimpleValueConverter<TSource, TTarget> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TSource)
            {
                try
                {
                    return ConvertBase((TSource)value);
                }
                catch (NotSupportedException) { }
            }
            return DependencyProperty.UnsetValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is TTarget)
            {
                try
                {
                    return ConvertBackBase((TTarget)value);
                }
                catch (NotSupportedException) { }
            }
            return DependencyProperty.UnsetValue;
        }
        protected abstract TTarget ConvertBase(TSource input);
        protected virtual TSource ConvertBackBase(TTarget input)
        {
            throw new NotSupportedException();
        }
    }

    public class FuncValueConverter<TSource, TTarget> : SimpleValueConverter<TSource, TTarget>
    {
        public FuncValueConverter(Func<TSource, TTarget> func)
        {
            Contract.Requires(func != null);
            m_func = func;
        }
        protected override TTarget ConvertBase(TSource input)
        {
            return m_func(input);
        }
        private readonly Func<TSource, TTarget> m_func;
    }
}
