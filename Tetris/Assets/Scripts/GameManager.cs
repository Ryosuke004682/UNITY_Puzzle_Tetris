using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    const int FIELD_WIDTH  = 10;
    const int FIELD_HEIGHT = 20;
    const int SCORE_LINE   = 100;

    [SerializeField] List<BlockController> prefabBlocks;
    
    //����̗������x
    [SerializeField] private float startFallTimerMax;

    [SerializeField] Text textScore;
    [SerializeField] GameObject panelResult;

    [SerializeField] AudioClip seHit;
    [SerializeField] AudioClip seDelete;

    BlockController nextBlock;
    BlockController currentBlock;

    Transform[,] fieldTiles;

    int currentScore;
    float currentGameTime;

    AudioSource audioSource;


    private void Start( )
    {
        fieldTiles = new Transform[FIELD_WIDTH, FIELD_HEIGHT];

        audioSource = GetComponent<AudioSource>();

        SetupNextBlock();
        SpawnBlock();

        currentScore = 0;
        textScore.text = "" + currentScore;

        panelResult.SetActive( false );
    }

    private void Update( )
    {
        //�o�ߎ���
        currentGameTime += Time.deltaTime;

        if (currentBlock.enabled) return;

        foreach(Transform item in currentBlock.transform)
        {
            Vector2Int index = GetIndexPosition(item.position);
            fieldTiles[index.x, index.y] = item;

            //�Q�[���I�[�o�[
            if(index.y > FIELD_HEIGHT - 2)
            {
                panelResult.SetActive( true );
                enabled = false;
            }
        }

        DeleteLines();
        SpawnBlock();


        audioSource.PlayOneShot(seHit);

    }


    /// <summary>
    /// ���[���h���W���C���f�b�N�X���W�ɕϊ�
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    Vector2Int GetIndexPosition(Vector3 position)
    {
        Vector2Int index = new Vector2Int();

        index.x = Mathf.RoundToInt(position.x - 0.5f) + FIELD_WIDTH  / 2;
        index.y = Mathf.RoundToInt(position.y - 0.5f) + FIELD_HEIGHT / 2;

        return index;
    }


    /// <summary>
    /// �ړ��\���ǂ���
    /// </summary>
    /// <param name="blockTransform"></param>
    /// <returns></returns>
    public bool IsMovable(Transform blockTransform)
    {
        foreach(Transform item in blockTransform)
        {
            Vector2Int index = GetIndexPosition(item.position);

            if (index.x < 0 || FIELD_WIDTH - 1 < index.x || index.y < 0)
            {
                return false;
            }

            if(GetFieldTile(index))
            {
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// ���̃u���b�N�̍쐬
    /// </summary>
    private void SetupNextBlock()
    {

        int randomCreate = Random.Range(0 , prefabBlocks.Count);

        Vector3 setupPosition = new Vector3(2.5f , 11.0f , 0.0f);

        //�u���b�N����
        BlockController prefab = prefabBlocks[randomCreate];
        nextBlock = Instantiate(prefab , setupPosition , Quaternion.identity);

        //���Ԍo�߂ɂ���ė������x�𑬂�����B
        float fallTime = startFallTimerMax;
        
        if(currentGameTime > 50)
        {
            fallTime = startFallTimerMax * 0.1f;
        }
        else if(currentGameTime > 30)
        {
            fallTime = startFallTimerMax * 0.3f;
        }
        else if(currentGameTime > 5)
        {
            fallTime = startFallTimerMax * 0.4f;
        }


        nextBlock.Init(this , fallTime);
        nextBlock.enabled = false;//�����Ȃ��悤�ɌŒ�
    }

    /// <summary>
    /// �u���b�N���t�B�[���h��
    /// </summary>
    private void SpawnBlock()
    {
        Vector3 spawnPosition = new Vector3(0.5f , 8.5f , 0.0f);

        //�u���b�N���Z�b�g
        currentBlock = nextBlock;
        currentBlock.transform.position = spawnPosition;

        currentBlock.enabled = true;

        SetupNextBlock();//���̃u���b�N���Z�b�g
    }


    /// <summary>
    /// �t�B�[���h�̃u���b�N������
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Transform GetFieldTile(Vector2Int index)
    {
        if(index.x < 0 || FIELD_WIDTH - 1 < index.x ||
            index.y < 0 || FIELD_HEIGHT - 1 < index.y)
        {
            return null;
        }

        return fieldTiles[index.x, index.y];
    }


    /// <summary>
    /// �폜�\���ǂ���
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    bool IsDeleteLine(int y)
    {
        for(int x = 0; x < FIELD_WIDTH; x++)
        {
            if (!fieldTiles[x, y]) return false;
        }

        return true;
    }

    /// <summary>
    /// ���C�����폜
    /// </summary>
    private void DeleteLine(int y )
    {
        for(int x = 0; x < FIELD_WIDTH; x++)
        {
            Destroy(fieldTiles[x , y].gameObject);
        }
    }

    /// <summary>
    /// ���C����������
    /// </summary>
    /// <param name="startY"></param>
    void FallLine( int startY )
    {
        //�w�肵�����C����S��
        for(int y = startY + 1; y < FIELD_HEIGHT; y++)
        {
            for(int x = 0; x < FIELD_WIDTH; x++)
            {
                if (!fieldTiles[x, y]) continue;

                //���[���h���W�X�V
                fieldTiles[x, y].position += Vector3.down;
                //�����f�[�^�X�V
                fieldTiles[x, y - 1] = fieldTiles[x, y];
                fieldTiles[x, y]     = null;
            }
        }
    }


    /// <summary>
    /// ���C���폜
    /// </summary>
    void DeleteLines()
    {
        bool isPlaySE = false;

        //�ォ�璲�ׂ�
        for(int y = FIELD_HEIGHT - 1; y >= 0; y--)
        {
            //���C����������Ă邩�ǂ����̃`�F�b�N
            if (!IsDeleteLine(y)) continue;

            DeleteLine(y);//���C�����폜
            FallLine(y)  ;//���C�������ɉ�����

            currentScore += SCORE_LINE;
            textScore.text = "" + currentScore;

            isPlaySE = true;
        }

        if(isPlaySE)
        {
            audioSource.PlayOneShot(seDelete);
        }

    }

    /// <summary>
    /// ���g���C�{�^���̍쐬
    /// </summary>
    public void OnClickRetry()
    {
        SceneManager.LoadScene("TetrisScene");
    }

}
