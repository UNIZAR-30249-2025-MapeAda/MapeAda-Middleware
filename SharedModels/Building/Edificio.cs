namespace MapeAda_Middleware.SharedModels.Building;

public sealed record Edificio
{
    public Edificio(Porcentaje porcentajeUsoMaximo, Calendario calendarioApertura)
    {
        PorcentajeUsoMaximo = porcentajeUsoMaximo;
        CalendarioApertura = calendarioApertura;
    }

    internal Edificio()
    {
    }

    public Porcentaje PorcentajeUsoMaximo { get; init; }
    public Calendario CalendarioApertura { get; init; }
}

