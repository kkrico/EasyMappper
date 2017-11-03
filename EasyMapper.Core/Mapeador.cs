using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EasyMapper.Core
{
    public static class Mapeador
    {
        private static readonly Dictionary<Type, PropertyInfo[]> CacheDictionary;

        static Mapeador()
        {
            CacheDictionary = new Dictionary<Type, PropertyInfo[]>();
        }

        public static TResultado MapearPara<TOrigem, TResultado>(TOrigem origem)
            where TResultado : class, new()
            where TOrigem : class
        {
            if (origem == null) return null;


            var propriedadesOrigem = GetPropriedadesDoCache(typeof(TOrigem));
            var propriedadesDestino = GetPropriedadesDoCache(typeof(TResultado));

            var resultado = new TResultado();

            foreach (var propDestino in propriedadesDestino)
            {
#if DEBUG
                var isMapeada = false;
#endif
                var propriedadeEncontradaNaOrigem =
                    propriedadesOrigem.FirstOrDefault(
                        p => string.Compare(p.Name, propDestino.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
                var propriedadeEncontradaNoDestino =
                    propriedadesDestino.FirstOrDefault(
                        p => string.Compare(p.Name, propDestino.Name, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (propriedadeEncontradaNoDestino != null && propriedadeEncontradaNaOrigem != null)
                {
                    if (propriedadeEncontradaNoDestino.CanWrite && propriedadeEncontradaNaOrigem.PropertyType.IsAssignableFrom(propDestino.PropertyType))
                    {
                        var valor = propriedadeEncontradaNaOrigem
                            .GetValue(origem, null);
                        propDestino.SetValue(resultado, valor, null);
#if DEBUG
                        isMapeada = true;
#endif
                    }
                }

#if DEBUG
                if (!isMapeada)
                    Debug.WriteLine("ATENÇÃO: Propriedade {0} do tipo {1} não mapeada".ToUpperInvariant(), propDestino.Name, typeof(TResultado).Name);
#endif
            }

            return resultado;
        }

        private static PropertyInfo[] GetPropriedadesDoCache(Type type)
        {
            if (CacheDictionary.ContainsKey(type))
            {
                return CacheDictionary[type];
            }

            var propriedades = type.GetProperties();
            CacheDictionary.Add(type, propriedades);

            return propriedades;
        }
    }
}
