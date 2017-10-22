using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EasyMapper.Core
{
    public class Mapeador
    {
        public static TResultado MapearPara<TOrigem, TResultado>(TOrigem origem)
           where TResultado : class
           where TOrigem : class
        {
            if (origem == null) return null;

            var tipoOrigem = origem.GetType();
            var tipoDestino = typeof(TResultado);

            var propriedadesOrigem = tipoOrigem.GetProperties();
            var propriedadesDestino = tipoDestino.GetProperties();
            var tiposNaoSuportados = new[] { typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(IEnumerable<>) };

            var resultado = typeof(TResultado) == typeof(string) ? origem : Activator.CreateInstance(typeof(TResultado));

            foreach (var propDestino in propriedadesDestino)
            {
#if DEBUG
                var isMapeada = false;
#endif
                if (propriedadesOrigem.Any(p => string.Compare(p.Name, propDestino.Name, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                    p.PropertyType.Name == propDestino.PropertyType.Name))
                {
                    var propriedadeEncontradaNaOrigem =
                        propriedadesOrigem.First(
                            p => string.Compare(p.Name, propDestino.Name, StringComparison.InvariantCultureIgnoreCase) == 0 && p.GetType() == propDestino.GetType());
                    var propriedadeEncontradaNoDestino =
                        propriedadesDestino.First(
                            p => string.Compare(p.Name, propDestino.Name, StringComparison.InvariantCultureIgnoreCase) == 0 && p.GetType() == propDestino.GetType());
                    var podeSerEscrita = propriedadeEncontradaNoDestino.CanWrite &&
                                         (!propDestino.PropertyType.IsGenericType || propDestino.PropertyType.GetGenericTypeDefinition() != typeof(IEnumerable<>) &&
                                          propDestino.PropertyType.IsGenericType &&
                                          !tiposNaoSuportados.Contains(propDestino.PropertyType.GetGenericTypeDefinition()));
                    if (podeSerEscrita)
                    {
                        var propertyInfo = origem.GetType()
                            .GetProperty(propriedadeEncontradaNaOrigem.Name);
                        if (propertyInfo != null)
                        {
                            var valor = propertyInfo
                                .GetValue(origem, null);
                            propDestino.SetValue(resultado, valor, null);
                        }
#if DEBUG
                        isMapeada = true;
#endif
                    }

#if DEBUG
                    isMapeada = podeSerEscrita;
#endif
                }
#if DEBUG
                if (!isMapeada)
                    Debug.WriteLine("ATENÇÃO: Propriedade {0} do tipo {1} não mapeada".ToUpperInvariant(), propDestino.Name, tipoDestino.Name);
#endif
            }

            return resultado as TResultado;
        }

        /// <summary>
        /// Mapea Enumerable. Está com constraint para class
        /// Refatorar para adicionar isto ao mapeador e ser recursivo
        /// </summary>
        /// <typeparam name="TOrigem"></typeparam>
        /// <typeparam name="TDestino"></typeparam>
        /// <param name="listaOrigem"></param>
        /// <returns></returns>
        public static IEnumerable<TDestino> MapearEnumerable<TOrigem, TDestino>(IEnumerable<TOrigem> listaOrigem)
            where TDestino : class
            where TOrigem : class
        {
            return MapearEnumerable<TOrigem, TDestino>(listaOrigem, null);
        }

        public static IEnumerable<TDestino> MapearEnumerable<TOrigem, TDestino>(IEnumerable<TOrigem> listaOrigem, Action<TDestino, TOrigem> customMapeador)
            where TDestino : class
            where TOrigem : class
        {
            var resultado = new List<TDestino>();
            foreach (var itemOrigem in listaOrigem)
            {
                var tmp = Mapeador.MapearPara<TOrigem, TDestino>(itemOrigem);

                if (customMapeador != null)
                {
                    customMapeador(tmp, itemOrigem);
                }

                resultado.Add(tmp);


            }

            return resultado;
        }
    }
}
