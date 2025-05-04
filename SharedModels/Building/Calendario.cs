namespace MapeAda_Middleware.SharedModels.Building;

public record Calendario(IEnumerable<HorarioApertura> HorariosApertura, Intervalo IntervaloPorDefecto, Dias DiasPorDefecto);
