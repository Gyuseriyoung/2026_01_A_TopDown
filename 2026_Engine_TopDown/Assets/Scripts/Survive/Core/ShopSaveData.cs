/// <summary>
/// JSON으로 직렬화되어 변환될 상점 데이터 바구니 구조체
/// </summary>
[System.Serializable]
public class ShopSaveData
{
    public int dmgLevel = 0;
    public int speedLevel = 0;
    public int fireRateLevel = 0;
    public int penetrationLevel = 0; // ⭐️ 관통 레벨 저장용 슬롯 필수
}