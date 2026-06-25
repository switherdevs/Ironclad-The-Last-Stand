using UnityEngine;
using System.Collections;

public class CaptainSkill : MonoBehaviour
{
    public GameObject damageZone;

    public void ActivateSkill()
    {
        StartCoroutine(SkillRoutine());
    }

    IEnumerator SkillRoutine()
    {
        damageZone.SetActive(true);

        yield return new WaitForSeconds(1f);

        damageZone.SetActive(false);
    }
}