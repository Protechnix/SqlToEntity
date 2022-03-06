using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace xUnitTests {
    public static class Extension {
        public static bool EqualValues(this object self, object other) {
            if (ReferenceEquals(self, other)) return true;
            if (self == null || other == null) return false;
            if (self.GetType() != other.GetType()) return false;

            if (!self.GetType().IsClass) return self.Equals(other);
            if (self.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEquatable<>))) return self.Equals(other);

            var properties = self.GetType().GetProperties();
            foreach (var property in properties) {
                var selfValue = property.GetValue(self);
                var otherValue = property.GetValue(other);

                if (self.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))) {
                    var outerIndex = 0;
                    foreach (var selfElement in (IEnumerable) self) {
                        var innerIndex = 0;
                        foreach (var otherElement in ((IEnumerable) other)) {
                            if (innerIndex == outerIndex) {
                                if (!otherElement.EqualValues(selfElement)) return false;
                                break;
                            }

                            innerIndex++;
                        }

                        outerIndex++;
                    }

                    break;
                }

                if (!otherValue.EqualValues(selfValue)) return false;
            }

            return true;
        }
    }
}