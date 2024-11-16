using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Gera o UI dos controles do foguete, com base no numero de estagios que o foguete tem
Controla qual parte do foguete esta ativa no moment, e controla a transicao de estagios
*/
public class StageControllerBuilder : MonoBehaviour
{
    [SerializeField] private RocketController rc;
    [SerializeField] private GameObject stageControllerPrefab;
    [SerializeField] private Slider altitudeSlider;
    private LinkedList<StageController> stageControllers;
    private RectTransform rocketTip;

    void Start()
    {
        rocketTip = transform.GetChild(0).GetComponent<RectTransform>();

        stageControllers = new LinkedList<StageController>();
        RocketController currentStage = rc;
        int stageIndex = 0;
        while(currentStage != null)
        {
            RectTransform scRect = Instantiate(stageControllerPrefab, transform).GetComponent<RectTransform>();
            scRect.anchoredPosition = new Vector2(210*stageIndex, 0);

            StageController sc = scRect.GetComponent<StageController>();
            stageControllers.AddLast(sc);
            sc.HookUp(currentStage);

            if(stageControllers.Last.Previous != null)
            {
                Button button = sc.progressButton;
                button.onClick.AddListener(() => 
                { 
                    button.gameObject.SetActive(false);
                    StartCoroutine(ProgressStage());
                });
            }

            sc.progressButton.gameObject.SetActive(false);
            sc.ToggleSliders(false);

            currentStage = currentStage.nextStage != null ? currentStage.nextStage.GetComponent<RocketController>() : null;
            stageIndex++;
        }
        rocketTip.anchoredPosition = new Vector2(210*stageIndex, 0);
        UpdateStageControllerUI();
    }

    void Update() => stageControllers.First.Value.UpdateAltitudeSlider(altitudeSlider);

    private IEnumerator ProgressStage()
    {
        stageControllers.First.Value.DetachNextStage();

        yield return MoveRectController(0.25f, new Vector2(0, -420), stageControllers.First.Value.GetComponent<RectTransform>());
        Destroy(stageControllers.First.Value.gameObject);
        stageControllers.RemoveFirst();

        foreach(StageController sc in stageControllers) 
            StartCoroutine(MoveRectController(0.25f, new Vector2(-210,0), sc.GetComponent<RectTransform>()));
        StartCoroutine(MoveRectController(0.25f, new Vector2(-210,0), rocketTip));
        yield return new WaitForSeconds(0.5f);

        UpdateStageControllerUI();
    }

    private void UpdateStageControllerUI()
    {
        stageControllers.First.Value.ToggleSliders(true);
        stageControllers.First.Value.HookThrustSlider();

        StageController next = stageControllers.First.Next?.Value;
        if(next != null) next.progressButton.gameObject.SetActive(true);
    }

    private IEnumerator MoveRectController(float duration, Vector2 movement, RectTransform rect)
    {
        Vector2 from = rect.anchoredPosition;
        Vector2 to = from + movement;
        float timer = 0;
        while(timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;

            rect.anchoredPosition = Vector2.Lerp(from, to, timer/duration);
        }
    }
}
