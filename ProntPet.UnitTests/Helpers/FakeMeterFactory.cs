using System.Diagnostics.Metrics;
using Moq;

namespace ProntPet.UnitTests.Helpers;

/// <summary>
/// Os Services recebem <see cref="IMeterFactory"/> via DI para criar métricas customizadas
/// (Counters). Nos testes unitários não nos importamos com a métrica em si — só precisamos
/// que a chamada a "Create" não quebre a construção do Service. Esse helper devolve um Mock
/// que sempre retorna um Meter real (e descartável), independente do MeterOptions recebido.
/// </summary>
public static class FakeMeterFactory
{
    public static IMeterFactory Create()
    {
        var mock = new Mock<IMeterFactory>();
        mock.Setup(m => m.Create(It.IsAny<MeterOptions>()))
            .Returns(new Meter("ProntPet.UnitTests"));

        return mock.Object;
    }
}
