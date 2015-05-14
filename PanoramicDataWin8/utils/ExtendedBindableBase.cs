using System;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.Mvvm;

namespace PanoramicDataWin8.utils
{
    public class ExtendedBindableBase : BindableBase
    {
        public string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return me.Member.Name;
        }
    }
}
