namespace TheLastStand.Model;

public interface ILightFogSupplier
{
	bool CanLightFogExistOnSelf { get; }

	bool CanLightFogSupplierMove { get; }

	bool IsLightFogSupplierMoving { get; set; }

	LightFogSupplierMoveDatas LightFogSupplierMoveDatas { get; set; }
}
