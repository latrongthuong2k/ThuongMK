using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;


[RequireComponent(typeof(ClearSceneView))]
public class ClearSceneManager : MonoBehaviour
{

    private CommonPresenter _commonPresenter;

    private ClearSceneView _clearSceneView;

    private QuestSceneView _questSceneView;

    void Start()
    {

        // fade In 
        _commonPresenter = CommonManager.CommonGameManager.GetComponent<CommonPresenter>();
        _clearSceneView = GetComponent<ClearSceneView>();
        _commonPresenter.FadeIn();


        // button
        _clearSceneView.NextButton
                          .OnClickAsObservable()
                          .ThrottleFirst(System.TimeSpan.FromMilliseconds(1000))
                          .Subscribe(_ => _commonPresenter.SceneChange("QuestScene").Forget())
                          .AddTo(this);
        //
        



    }
}