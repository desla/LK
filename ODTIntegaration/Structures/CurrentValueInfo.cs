namespace Alvasoft.ODTIntegration.Structures
{
    /// <summary>
    /// Описание текущего значения.
    /// </summary>
    public class CurrentValueInfo
    {
        /// <summary>
        /// Имя ОРС-тега в ОРС-сервере.
        /// </summary>
        public string OpcItemName { get; set; }

        /// <summary>
        /// Имя типа в ИТС.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Имя объекта и ИТС.
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Имя значения в ИТС.
        /// </summary>
        public string DataName { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CurrentValueInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                var hashCode = 
                    (TypeName != null ? TypeName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ 
                    (ObjectName != null ? ObjectName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ 
                    (DataName != null ? DataName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
