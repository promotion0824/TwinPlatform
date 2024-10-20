using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Willow.DataValidation
{
    public static class Extensions
    {
        public static bool Validate(this object obj, IList<(string Name, string Message)> errors, string prefix = null)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where( p=> p.CanRead );
            var validationContext = new ValidationContext(obj);

            prefix ??= "";

            foreach(var property in properties)
            {
                try
                { 
                    var annotations = property.GetCustomAttributes(typeof(ValidationAttribute));
                    var name        = property.Name;
                    var value       = property.GetGetMethod().Invoke(obj, null);
                    var errCount    = errors.Count;

                    foreach(var annotation in annotations)
                    {
                        if(value == null)
                        { 
                            if(annotation.GetType().Name.StartsWith("Required"))
                            { 
                                var required = annotation as ValidationAttribute;

                                if(required.GetValidationResult(value, validationContext) != ValidationResult.Success)
                                { 
                                    var msg = required.FormatErrorMessage(name);

                                    if(!msg.StartsWith("ERR_"))
                                        msg = prefix + msg;

                                    errors.Add(($"{prefix}{name}", msg));
                                    break;
                                }
                            }
                        }

                        if(annotation is ValidationAttribute validationAttribute)
                        {
                            if(annotation is RequiredAttribute && (!(value is string) && value is IEnumerable listReq))
                            {
                                if(listReq == null)
                                { 
                                    var msg = validationAttribute.FormatErrorMessage(name);

                                    if(!msg.StartsWith("ERR_"))
                                        msg = prefix + msg;

                                    errors.Add(($"{prefix}{name}", msg));
                                    break;
                                }

                                continue;
                            }

                            var error = false;

                            if(validationAttribute.RequiresValidationContext)
                            { 
                                var result = validationAttribute.GetValidationResult(value, new ValidationContext(obj));
                                
                                if(result != ValidationResult.Success)
                                { 
                                    var msg = result.ErrorMessage;

                                    if(!msg.StartsWith("ERR_"))
                                        msg = prefix + msg;

                                    errors.Add(($"{prefix}{name}", msg));
                                    error = true;
                                }
                            }
                            else if(!validationAttribute.IsValid(value))
                            { 
                                    var msg = validationAttribute.FormatErrorMessage(name);

                                    if(!msg.StartsWith("ERR_"))
                                        msg = prefix + msg;

                                errors.Add(($"{prefix}{name}", msg));
                                error = true;
                            }

                            if(error && validationAttribute is RequiredAttribute)
                                break;
                        }
                    }

                    if(value == null)
                        continue;

                    // If this is a list or class object then check children/properties
                    if(errCount == errors.Count && !(value is string))
                    { 
                        if(value is IEnumerable children)
                        {
                            var i = 0;

                            foreach(var child in children)
                                child.Validate(errors, $"{name}[{i++}].");

                            continue;
                        }
                        else if(!(value is string) && value.GetType().IsClass)
                        { 
                            value.Validate(errors, $"{name}.");

                            continue;
                        }
                    }
                }
                catch
                {
                    // Just ignore it
                }
            }            
            
            return errors.Count == 0;
        }

        public static IList<object> ToObjectList(this object obj)
        {
            if(obj is IList<object> list)
                return list;

            if(obj is IEnumerable enm)
            {
                var list2 = new List<object>();

                foreach(var item in enm)
                    list2.Add(item);

                return list2;
            }

            return null;
        }

        public static T GetValue<T>(this object obj, string propertyName)
        {
            var type    = obj.GetType();
            var property = type.GetProperty(propertyName);

            if(property == null)
                return default(T);

            var val = property.GetValue(obj);

            if(val != null && !val.GetType().IsEquivalentTo(typeof(T)))
                return (T)Convert.ChangeType(val, typeof(T));

            return (T)val;
        }
    }
}
