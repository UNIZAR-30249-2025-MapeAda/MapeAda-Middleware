namespace MapeAda_Middleware.SharedModels.Building;

[Flags]
public enum Dias
{
    Lunes=1, 
    Martes=2,
    Miercoles=4,
    Jueves=8,
    Viernes=16,
    Sabado=32,
    Domingo=64
}
