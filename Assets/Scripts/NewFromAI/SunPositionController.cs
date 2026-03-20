using UnityEngine;
using System;

public class SunPositionController : MonoBehaviour
{
    [Header("Location")]
    [Range(-90f, 90f)] public float latitude = 43.25f;   // Алматы
    [Range(-180f, 180f)] public float longitude = 76.95f;

    [Header("Date & Time")]
    public int year = 2025;
    [Range(1, 12)] public int month = 6;
    [Range(1, 31)] public int day = 21;
    [Range(0f, 24f)] public float timeOfDay = 12f;

    [Header("References")]
    public Light sunLight;

    void Update()
    {
        UpdateSun();
    }

    public void UpdateSun()
    {
        DateTime date = new DateTime(year, month, day);
        int dayOfYear = date.DayOfYear;

        float latRad = Mathf.Deg2Rad * latitude;

        // Солнечное склонение
        float declination = 23.45f * Mathf.Sin(Mathf.Deg2Rad * (360f / 365f * (dayOfYear - 81)));
        float declRad = Mathf.Deg2Rad * declination;

        // Часовой угол
        float hourAngle = (timeOfDay - 12f) * 15f;
        float hourRad = Mathf.Deg2Rad * hourAngle;

        // Высота солнца
        float altitude = Mathf.Asin(
            Mathf.Sin(latRad) * Mathf.Sin(declRad) +
            Mathf.Cos(latRad) * Mathf.Cos(declRad) * Mathf.Cos(hourRad)
        );

        // Азимут
        float azimuth = Mathf.Acos(
            (Mathf.Sin(declRad) - Mathf.Sin(altitude) * Mathf.Sin(latRad)) /
            (Mathf.Cos(altitude) * Mathf.Cos(latRad))
        );

        float altitudeDeg = Mathf.Rad2Deg * altitude;
        float azimuthDeg = Mathf.Rad2Deg * azimuth;

        if (timeOfDay > 12f)
            azimuthDeg = 360f - azimuthDeg;

        // Поворот солнца
        //sunLight.transform.rotation = Quaternion.Euler(altitudeDeg, azimuthDeg, 0f);
        sunLight.transform.rotation = Quaternion.Euler(altitudeDeg, azimuthDeg - 180f, 0f);
    }
}
