using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OstatniaScena : MonoBehaviour
{
    [SerializeField]
    protected List<EvilBun> Actors;
    
    [SerializeField]
    protected AudioSource ImportantSoundEffect;

    [SerializeField]
    protected float Rotation_time;

    [SerializeField]
    protected int Rotation_Step_Count;


    void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.Player.MovementBlocked = true;
        StartCoroutine(RotationAnimation());
        StartCoroutine(PlaySound());

    }

    IEnumerator RotationAnimation()
    {
        float delay = Rotation_time/Rotation_Step_Count;
        float lerp_time = 0;
        for(int i = 0; i <= Rotation_Step_Count; i++)
        {
            foreach(EvilBun bunny in Actors)
            {
                bunny.RotateByStepTowards(GameManager.Instance.Player.transform, lerp_time);    
            }
            
            lerp_time += 1.0f/Rotation_Step_Count;
            Debug.Log("print time " + lerp_time);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(1);

        foreach(EvilBun bunny in Actors)
        {
            bunny.Hop(GameManager.Instance.Player.transform);    
        }

        yield return new WaitForSeconds(0.4f);
        GameManager.Instance.Blackscreen.color = Color.black;
        yield return new WaitForSeconds(2);

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(0.5f);
        ImportantSoundEffect.Play();
    }
}
