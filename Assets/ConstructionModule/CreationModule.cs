using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Данный класс позволяет превратить все дочерние объекты вашей конструкиции в детали, из которых нужно её собирать. 
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CreationModule : MonoBehaviour
{
    [Range(1, 10), Tooltip("Сила прыжка каждой детали")] public float jumpForce = 2;
    [Range(0.1f, 2), Tooltip("Задержка между прыжком каждой детали")] public float jumpDelay = 0.3f;
    [Range(0, 4), Tooltip("Стартовая скорость сборки, которая в итоге плавно будет возрастать до максимальной")]
    public float startBuildSpeed = 1;
    [Range(1, 4), Tooltip("Во сколько раз максимальная скорость сборки больше стартовой")]
    public float maxSpeedMultiplicator = 2;
    [Range(0, 4), Tooltip("Интенсивность увеличения скорости сборки")]
    public float changeSpeedMultiplicator = 2;
    [Tooltip("Доступно для постройки сразу")] public bool activeOnPlay;
    [Tooltip("После того, как конструкция полностью собрана, она подпрыгнет. Это здорово помогает понять, когда нужно отпустить кнопку.")]
    public bool useFinalJump;
    [Range(1, 4), Tooltip("Высота финального прыжка")]
    public float finalJampHeight = 2;

    [Space(20), Tooltip("Пометить все дочерние объекты деталями")]
    public bool renewDetails;
    [Space(10), Tooltip("Случайная позиция для деталей")]
    public bool randomizeDetailsPositions;
    [Range(1, 10), Tooltip("Радиус для случайной позиции")]
    public float randomPosRadius = 2;
    [Space(10), Tooltip("Собрать детали в конструкицю")]
    public bool returnDetails;

    private List<ConstructionDetailScript> details;
    private ConstrictionState constructState;
    private Transform myTransform;
    Vector3 standartPos, FinalJumpPos;
    private int currentDetailIndex;
    private float maxBuildSpeed;
    private float currentBuildSpeed;

    private void Start()
    {
        myTransform = transform;
        standartPos = myTransform.position;
        FinalJumpPos = standartPos + myTransform.up * finalJampHeight;
        GetDetails();
        constructState = ConstrictionState.Disactive;
        currentDetailIndex = -1;
        currentBuildSpeed = startBuildSpeed;
        maxBuildSpeed = startBuildSpeed * maxSpeedMultiplicator;
        if(activeOnPlay)
        {
            Activate();
        }
    }
    private void Update()
    {
        CheckConstructStatus();
    }

    /// <summary>
    /// Включает возможность собрать из деталей что-то
    /// </summary>
    public void Activate()
    {
        if (constructState == ConstrictionState.Disactive)
        {
            CheckDetailsToJump();
            GetNextDetail();
        }
    }

    /// <summary>
    /// Позволяет вести стройку и остановить её
    /// </summary>
    /// <param name="value"></param>
    public void Build(bool value)
    {
        if (constructState != ConstrictionState.Disactive && constructState != ConstrictionState.BuildComplete)
        {
            constructState = value ? ConstrictionState.StartBuild : ConstrictionState.BuildError;
        }
    }

    /// <summary>
    /// Подготовить детали к прыжку (на них будет реагировать гравитация)
    /// </summary>
    private void CheckDetailsToJump()
    {
        foreach (var item in details)
        {
            item.rb.useGravity = true;
        }
    }

    /// <summary>
    /// Следит за состоянием конструкции
    /// </summary>
    private void CheckConstructStatus()
    {
        switch (constructState)
        {
            case ConstrictionState.DetailJump:
                CurrentDetailJump();
                break;
            case ConstrictionState.StartBuild:
                ToConstructType();
                break;
            case ConstrictionState.MoveCurrentDetail:
                MoveCurrentDetailToPosition();
                break;
            case ConstrictionState.CurrentDetailComplete:
                DetailInstalled();
                break;
            case ConstrictionState.BuildError:
                BuildError();
                break;
            case ConstrictionState.BuildComplete:
                FinalConstruction();
                break;
            case ConstrictionState.FinalJump:
                MoveConstructToPosition(currentDetailIndex == -2 ? FinalJumpPos : standartPos);
                break;
        }
    }
    /// <summary>
    /// Заставяет детали прыгать
    /// </summary>
    private void CurrentDetailJump()
    {
        Vector3 dir = Vector3.up;
        if (Vector3.Distance(myTransform.position, details[currentDetailIndex].transform.position) > 1)
        {
            dir += (myTransform.position - details[currentDetailIndex].transform.position).normalized * 4;
        }
        details[currentDetailIndex].rb.AddForce(dir.normalized * jumpForce, ForceMode.Impulse);
        constructState = 0;
        Invoke("GetNextDetail", jumpDelay);
    }
    /// <summary>
    /// Используется в методе CurrentDetailJump. Позволяет получить индекс следующей детали для того, чтобы выполнить прыжок
    /// </summary>
    private void GetNextDetail()
    {
        currentDetailIndex++;
        if(currentDetailIndex > details.Count-1)
        {
            currentDetailIndex = 0;
        }
        if (((int)constructState) < 2 || constructState == ConstrictionState.BuildError)
            constructState = ConstrictionState.DetailJump;
    }

    /// <summary>
    /// Берёт активную деталь и готовит её переносу на позицю. Активная деталь всегда будет иметь индекс = 0, поскольку все активные
    /// детали, установленные на позиции, удаляются из списка деталей.
    /// </summary>
    private void ToConstructType()
    {
        currentDetailIndex = 0;
        SetParamsForCurrentDetail(false, false, true, currentDetailIndex);
        constructState = ConstrictionState.MoveCurrentDetail;
    }

    /// <summary>
    /// Переносит текущуюю деталь на позицю
    /// </summary>
    private void MoveCurrentDetailToPosition()
    {
        Vector3 dir = details[0].targetPosition - details[0].transform.position;
        details[0].rb.velocity = Vector3.zero;

        if (dir.magnitude > Time.deltaTime * 3)
        {
            details[0].transform.position += dir.normalized * currentBuildSpeed * Time.deltaTime;
        }
        else
        {
            details[0].transform.position = details[0].targetPosition;
            details[0].transform.rotation = details[0].targetRotation;
            constructState = ConstrictionState.CurrentDetailComplete;
        }

        currentBuildSpeed += Time.deltaTime * changeSpeedMultiplicator;
        if (currentBuildSpeed > maxBuildSpeed)
        {
            currentBuildSpeed = maxBuildSpeed;
        }
    }

    /// <summary>
    /// Сробатывает, когда деталь установлена на позицию. Закрепляет её как физический объект, отбирает функционал детали
    /// и удаляет из списка деталей. После этого либо активной деталью становится следующая, либо стройка завершена.
    /// </summary>
    private void DetailInstalled()
    {
        ConstructionDetailScript detail = details[0];
        SetParamsForCurrentDetail(true, false, true, 0);
        details.Remove(detail);
        Destroy(detail);
        
        constructState = details.Count > 0 ? constructState = ConstrictionState.StartBuild : constructState = ConstrictionState.BuildComplete;
    }

    /// <summary>
    /// Срабатывает, если сборка прекратилась, но ещё н е была закончена
    /// </summary>
    private void BuildError()
    {
        currentBuildSpeed = startBuildSpeed;
        SetParamsForCurrentDetail(true, true, false, 0);
        currentDetailIndex = -1;
        constructState = ConstrictionState.DetailJump;
        GetNextDetail();
    }

    /// <summary>
    /// Заканчивает стройку
    /// </summary>
    private void FinalConstruction()
    {
        currentDetailIndex = -2;
        if (useFinalJump)
            constructState = ConstrictionState.FinalJump;
        else
            DisableThisConstruction();
    }
    /// <summary>
    /// Движение самой конструкции во время финального прыжка
    /// </summary>
    /// <param name="target"></param>
    private void MoveConstructToPosition(Vector3 target)
    {
        Vector3 dir = target - myTransform.position;
        if (dir.magnitude > Time.deltaTime * startBuildSpeed * Mathf.Abs(currentDetailIndex))
            myTransform.position += dir * Time.deltaTime * startBuildSpeed * Mathf.Abs(currentDetailIndex);
        else
        {
            myTransform.position = target;
            Invoke("ReturnConstructToStandartPos", jumpDelay);
            constructState = ConstrictionState.Delay;
        }
    }

    private void ReturnConstructToStandartPos()
    {
        if(currentDetailIndex == -2)
        {
            currentDetailIndex = -3;
            constructState = ConstrictionState.FinalJump;
        }
        else
            DisableThisConstruction();
    }
    private void DisableThisConstruction()
    {
        for (int i = 0; i < myTransform.childCount; i++)
        {
            ConstructionDetailScript detail = myTransform.GetChild(i).GetComponent<ConstructionDetailScript>();
            if (detail != null)
                Destroy(detail);
        }
        GetComponent<Collider>().enabled = false;
        Destroy(this, Time.deltaTime);
    }
 
    /// <summary>
    /// Настраивает значения физики для активной детали
    /// </summary>
    /// <param name="enebleCollider"></param>
    /// <param name="useGravity"></param>
    /// <param name="isKinematic"></param>
    private void SetParamsForCurrentDetail(bool enebleCollider, bool useGravity, bool isKinematic, int detailIndex)
    {
        details[detailIndex].GetComponent<Collider>().enabled = enebleCollider;
        details[detailIndex].rb.isKinematic = isKinematic;
        details[detailIndex].rb.useGravity = useGravity;
    }

    private void GetDetails()
    {
        details = new List<ConstructionDetailScript>();
        for (int i = 0; i < myTransform.childCount; i++)
        {
            ConstructionDetailScript detail = myTransform.GetChild(i).GetComponent<ConstructionDetailScript>();
            if (detail != null) details.Add(detail);
        }
    }

    /// <summary>
    /// Стирает все ссылки на детали, занова пробегается по всем вложенным объектам и превращает те из них, которые имеют Rigidbody,
    /// в детали
    /// </summary>
    private void RenewAllDetails()
    {
        details = new List<ConstructionDetailScript>();
        for (int i = 0; i < myTransform.childCount; i++)
        {
            GameObject obj = myTransform.GetChild(i).gameObject;
            if (obj.GetComponent<Rigidbody>() != null)
            {
                ConstructionDetailScript detail = obj.GetComponent<ConstructionDetailScript>();
                if (detail == null)
                {
                    detail = obj.AddComponent<ConstructionDetailScript>();
                }
                detail.rb = obj.GetComponent<Rigidbody>();

                detail.CheckTarget();
                details.Add(detail);
            }
        }
        EditorUtility.SetDirty(gameObject);
    }
    /// <summary>
    /// Располагает все детали конструкции в случайных позициях в радиусе
    /// </summary>
    private void RandomizeDetailsPositions()
    {
        for (int i = 0; i < myTransform.childCount; i++)
        {
            GameObject obj = myTransform.GetChild(i).gameObject;
            if (obj.GetComponent<Rigidbody>() != null)
            {
                ConstructionDetailScript detail = obj.GetComponent<ConstructionDetailScript>();
                if (detail != null)
                {
                    detail.transform.position = myTransform.position + Random.insideUnitSphere * randomPosRadius; 
                }
            }
        }
        EditorUtility.SetDirty(gameObject);
    }
    /// <summary>
    /// Располагает детали так, чтобы снова получилась собранная конструкиция (кок она была сохранена при получении деталей)
    /// </summary>
    private void DetailsToStartPositions()
    {
        for (int i = 0; i < myTransform.childCount; i++)
        {
            GameObject obj = myTransform.GetChild(i).gameObject;
            if (obj.GetComponent<Rigidbody>() != null)
            {
                ConstructionDetailScript detail = obj.GetComponent<ConstructionDetailScript>();
                if (detail != null)
                {
                    detail.transform.position = detail.targetPosition;
                    detail.transform.rotation = detail.targetRotation;
                }
            }
        }
        EditorUtility.SetDirty(gameObject);
    }

    private void OnDrawGizmos()
    {
        if(renewDetails)
        {
            RenewAllDetails();
            renewDetails = false;
        }

        if (randomizeDetailsPositions)
        {
            RandomizeDetailsPositions();
            randomizeDetailsPositions = false;
        }

        if (returnDetails)
        {
            DetailsToStartPositions();
            returnDetails = false;
        }
    }
}

public enum ConstrictionState
{
    Disactive = -1,
    Delay = 0,
    DetailJump = 1,
    StartBuild = 2,
    MoveCurrentDetail = 3,
    CurrentDetailComplete = 4,
    BuildError = 5,
    BuildComplete = 6,
    FinalJump = 7
}
