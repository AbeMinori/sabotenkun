using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using OpenCvSharp;
using UnityEngine.Video;
using System.Threading;
using UnityEditor.VersionControl;
using Task = System.Threading.Tasks.Task;
using UnityEngine.UIElements;


//namespace Opencv_test12

// Webカメラ
public class camcontroll3 : MonoBehaviour
{
    private static int camWidth = 1920;
    private static int camHeight = 1080;
    private static int FPS = 30;

    public SerialHandler serial;

    //カメラの位置
    public int cameraPosition_V = 1080-200;
    public int cameraPosition_H = 300;
    public int cameraPosition_V_offset = 200;
    public int cameraPosition_H_offset = 200;

    GameObject UICanvas;
    GameObject springSabo;
    GameObject summerSabo;
    GameObject autumnSabo;
    GameObject winterSabo;

    //mat
    Mat mat_V;
    Mat mat_H;
    Mat mat_grid;
    Mat hsver_V;
    Mat hsver_H;
    Mat bindata_V;
    Mat bindata_H;
    Mat whiteBindata;
    Mat gray_V;
    Mat gray_H;
    Mat bin_V;
    Mat bin_H;

    // UI
    RawImage rawImage_V;
    RawImage rawImage_H;
    RawImage rawImage_grid;
    WebCamTexture camtex_V;
    WebCamTexture camtex_H;


    //青
    private readonly static Scalar LOWER = new Scalar(120, 0, 0);
    private readonly static Scalar UPPER = new Scalar(255, 250, 100);
    private readonly static Scalar LOWEwhite = new Scalar(150, 150, 150);
    public Scalar UPPERwhite = new Scalar(255, 250, 250);
    //緑色
    //private readonly static Scalar LOWER = new Scalar(0, 130, 0);
    //private readonly static Scalar UPPER = new Scalar(155, 255, 155);
    //赤
    //private readonly static Scalar LOWER = new Scalar(0, 0, 130);
    //private readonly static Scalar UPPER = new Scalar(155, 155, 255);

    Point[][] contours_V;
    Point[][] contours_H;
    HierarchyIndex[] hierarchy_V;
    HierarchyIndex[] hierarchy_H;
    HierarchyIndex[] hierarchyIndexes_V;
    HierarchyIndex[] hierarchyIndexes_H;

    //面積用
    public int areaLow = 3000;
    public int areaUpp = 60000;

    public int lengthLow = 400;
    public int lengthUpp = 1200;

    public float aspectLow = 2.0f;

    //動画再生用
    public VideoPlayer videoPlayer;
    public VideoClip videoClip;
    public GameObject screen;

    //Videoplayer
    VideoPlayer videoOp;
    VideoPlayer videoSpring;
    VideoPlayer videoSummer;
    VideoPlayer videoAutumn;
    VideoPlayer videoWinter;
    VideoPlayer videoEnd;

    //さぼてんくんだけのVideoplayer
    VideoPlayer videoSpringSabo;
    VideoPlayer videoSummerSabo;
    VideoPlayer videoAutumnSabo;
    VideoPlayer videoWinterSabo;

    int countMov = 0;
    bool OnceInc = false;
    bool MouseClick = false;
    bool _overThreshold = false;
    bool _overThreshold_Once = false;
    bool _overThreshold_V = false;
    bool _overThreshold_H = false;
    bool _overWaitTime = false;
    bool _videoPlaying = false;
    bool _continueJudge = false;
    bool _videoEnd = false;
    bool _videoEnd_Once = false;
    bool _SaboPosition_OnceInc = false;
    bool _grid = false;
    bool _UI = false;

    bool _led = false;

    double blueArea_V = 0.0;
    double blueArea_H = 0.0;
    double systemTime = 0.0;

    Vec3b pix;
    //OpenCvSharp.Point[][] contours;
    //OpenCvSharp.HierarchyIndex[] hierarchyIndexes;


    // スタート時に呼ばれる
    void Start()
    {
        serial.OnDataReceived += OnDataReceived;
        UICanvas = GameObject.Find("Canvas");
        // Webカメラの開始
        this.rawImage_V = GameObject.Find("RawImage_v").GetComponent<RawImage>();
        this.rawImage_H = GameObject.Find("RawImage_h").GetComponent<RawImage>();
        this.rawImage_grid = GameObject.Find("grid").GetComponent<RawImage>();
        WebCamDevice[] devices = WebCamTexture.devices;
        for(var i=0; i<devices.Length; i++)
        {
            Debug.Log(devices[i].name);
        }
        this.camtex_V = new WebCamTexture(devices[2].name, camWidth, camHeight, FPS);
        this.camtex_H = new WebCamTexture(devices[1].name, camWidth, camHeight, FPS);
        //Debug.Log(devices[0].name);
        //Debug.Log(devices[1].name);

        this.camtex_V.Play();
        this.camtex_H.Play();
        this.rawImage_V.texture = this.camtex_V;
        this.rawImage_H.texture = this.camtex_H;
        this.rawImage_grid.texture = this.rawImage_grid.texture;

        ////動画
        //var videoPlayer = screen.AddComponent<VideoPlayer>();
        ////動画ソースの設定
        //videoPlayer.source = VideoSource.VideoClip;
        //videoPlayer.clip = videoClip;

        //var VideoPlayer = GetComponent<VideoPlayer>();
        //VideoPlayer.Pause();

        //動画再生用
        videoOp = GameObject.Find("op").GetComponent<VideoPlayer>();
        videoSpring = GameObject.Find("spring").GetComponent<VideoPlayer>();
        videoSummer = GameObject.Find("summer").GetComponent<VideoPlayer>();
        videoAutumn = GameObject.Find("autumn").GetComponent<VideoPlayer>();
        videoWinter = GameObject.Find("winter").GetComponent<VideoPlayer>();
        videoEnd = GameObject.Find("end").GetComponent<VideoPlayer>();

        videoSpringSabo = GameObject.Find("springSabo").GetComponent<VideoPlayer>();
        videoSummerSabo = GameObject.Find("summerSabo").GetComponent<VideoPlayer>();
        videoAutumnSabo = GameObject.Find("autumnSabo").GetComponent<VideoPlayer>();
        videoWinterSabo = GameObject.Find("winterSabo").GetComponent<VideoPlayer>();

        springSabo = GameObject.Find("springSabo");
        summerSabo = GameObject.Find("summerSabo");
        autumnSabo = GameObject.Find("autumnSabo");
        winterSabo = GameObject.Find("winterSabo");

    }

    void Update()
    {
        //mat = OpenCvSharp.Unity.TextureToMat(this.camtex);
        //this.rawImage.texture = OpenCvSharp.Unity.MatToTexture(mat);
        KeybordInput();


        //動画取り込み
        if (!_videoPlaying && !_videoEnd_Once)
        {
            mat_V = OpenCvSharp.Unity.TextureToMat(this.camtex_V);
            mat_H = OpenCvSharp.Unity.TextureToMat(this.camtex_H);
            hsver_V = new Mat();
            hsver_H = new Mat();
            mat_grid = OpenCvSharp.Unity.TextureToMat(new Texture2D(1920,1080));
            //mat_grid = OpenCvSharp.Unity.TextureToMat(this.camtex_H);

            //上限と下限を設定(青)
            bindata_V = mat_V.InRange(LOWER, UPPER);
            bindata_H = mat_H.InRange(LOWER, UPPER);
            //whiteBindata = mat.InRange(LOWEwhite, UPPERwhite);

            //グレースケールにする
            this.rawImage_V.texture = OpenCvSharp.Unity.MatToTexture(bindata_V);
            this.rawImage_H.texture = OpenCvSharp.Unity.MatToTexture(bindata_H);
            //this.rawImage.texture = OpenCvSharp.Unity.MatToTexture(whiteBindata);

            //輪郭抽出
            bindata_V.FindContours(out contours_V, out hierarchyIndexes_V, RetrievalModes.External,ContourApproximationModes.ApproxSimple);
            bindata_H.FindContours(out contours_H, out hierarchyIndexes_H, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            //whiteBindata.FindContours(out contours, out hierarchyIndexes, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            //輪郭を赤枠で囲う
            //mat.DrawContours(contours, -1, Scalar.Red, 2);

            //小さいの消す
            //GetComponent<RawImage>().texture = OpenCvSharp.Unity.MatToTexture(mat);
            //this.rawImage.texture = OpenCvSharp.Unity.MatToTexture(mat);

            // 中心の座標を計算する


            //キー入力、マウスクリック検知
            double maxArea_V = 0;
            double maxArea_H = 0;
            int largestContour_V = 0;
            int largestContour_H = 0;
            double areaSize_V = 0;
            double areaSize_H = 0;
            var rect_V = new OpenCvSharp.Rect();
            var rect_H = new OpenCvSharp.Rect();
            _overThreshold_Once = false;

            Point[][] bigArea_V = new Point[contours_V.GetLength(0)][];
            Point[][] bigArea_H = new Point[contours_H.GetLength(0)][];
            int areaCount_V = 0;
            int areaCount_H = 0;

            //Debug.Log("me");
            //得られた輪郭全てにおいて処理を行う
            //縦
            for (int i = 0; i < contours_V.GetLength(0); ++i)
            {
                if (hierarchyIndexes_V[i].Parent != -1)
                {
                    continue;
                }

                var array_V = contours_V[i].ToArray();  //i番目の輪郭を配列に格納
                var area_V = Cv2.ContourArea(array_V, false);   //i番目の輪郭の面積を格納
                var length_V = Cv2.ArcLength(array_V, true);
                rect_V = Cv2.BoundingRect(array_V);
                float aspect_V = (float)rect_V.Width / (float)rect_V.Height;
                //Debug.Log(length_V);

                if (area_V > maxArea_V)
                {
                    maxArea_V = area_V;
                    largestContour_V = i; //一番面積が大きい輪郭のインデックスを格納
                    //Debug.Log();

                }
                //Debug.Log(area);
                areaSize_V = area_V;
                //動画再生
                var VideoPlayer_V = GetComponent<VideoPlayer>();
                //VideoPlayer.Pause();
                if (area_V > areaLow && area_V < areaUpp && length_V > lengthLow && length_V < lengthUpp && aspect_V >= aspectLow)
                {
                    //一定サイズの輪郭を別配列に保存
                    bigArea_V[areaCount_V] = contours_V[i];

                    //Debug.Log(contours[i].Length + "," + bigArea[areaCount].Length);

                    //一定サイズの輪郭の保存個数をカウントアップ
                    areaCount_V += 1;

                    _overThreshold_V = true;

                    if (_UI)
                    {
                        Cv2.Rectangle(mat_grid, new Point(rect_V.X, rect_V.Y), new Point(rect_V.X + rect_V.Width, rect_V.Y + rect_V.Height), Scalar.Green, 2);

                    }


                }
            }

            //横
            for (int i = 0; i < contours_H.GetLength(0); ++i)
            {
                if (hierarchyIndexes_H[i].Parent != -1)
                {
                    continue;
                }

                var array_H = contours_H[i].ToArray();  //i番目の輪郭を配列に格納
                var area_H = Cv2.ContourArea(array_H, false);   //i番目の輪郭の面積を格納

                var length_H = Cv2.ArcLength(array_H, true);
                rect_H = Cv2.BoundingRect(array_H);
                float aspect_H = (float)rect_H.Width / (float)rect_H.Height;

                if (area_H > maxArea_H)
                {
                    maxArea_H = area_H;
                    largestContour_H = i; //一番面積が大きい輪郭のインデックスを格納
                    //Debug.Log();

                }
                //Debug.Log(area);
                areaSize_H = area_H;
                //動画再生
                var VideoPlayer_H = GetComponent<VideoPlayer>();
                //VideoPlayer.Pause();
                if (area_H > areaLow && area_H < areaUpp && length_H > lengthLow && length_H < lengthUpp && aspect_H >= aspectLow)
                {
                    //一定サイズの輪郭を別配列に保存
                    bigArea_H[areaCount_H] = contours_H[i];

                    //Debug.Log(contours[i].Length + "," + bigArea[areaCount].Length);

                    //一定サイズの輪郭の保存個数をカウントアップ
                    areaCount_H += 1;

                    _overThreshold_H = true;
                    if (_UI)
                    {
                        Cv2.Rectangle(mat_grid, new Point(rect_H.X, rect_H.Y), new Point(rect_H.X + rect_H.Width, rect_H.Y + rect_H.Height), Scalar.Yellow, 2);

                    }
                }
            }

            if(_overThreshold_H && _overThreshold_V)
            {
                //一定サイズの輪郭が取得された瞬間ビデオを一回だけ流すためにフラグを立てる
                _overThreshold_Once = true;

                //一度も一定サイズの輪郭が取得されてないかつビデオ再生中でなければここに入る
                if (!_overThreshold && !_videoPlaying)
                {
                    //一定サイズを超えたというフラグを立てる
                    _overThreshold = true;
                    OnceInc = false;
                    //一定サイズを超えた瞬間タイマーの値を0にする
                    systemTime = 0.0;
                }

                _overThreshold_H = false;
                _overThreshold_V = false;
            }

            //さぼてんくんの投影場所を変更する
            //一番大きい面積を持ってくる
            //縦
            Point[][] bigArea_copy_V = new Point[areaCount_V][];
            for(var i=0; i<areaCount_V; i++)
            {
                bigArea_copy_V[i] = bigArea_V[i];
            }

            //中心を求めて点を打つ
            int max_x_V = 0;
            int min_x_V = camWidth;
            if (areaCount_V >= 1)
            {
                for (int i = 0;i < bigArea_copy_V.Length;i++)
                {
                    for (int j = 0;j < bigArea_copy_V[i].Length;j++)
                    {
                        //Debug.Log(bigArea_copy[i][j].X);
                        if(max_x_V < bigArea_copy_V[i][j].X)
                        {
                            max_x_V = bigArea_copy_V[i][j].X;
                        }
                        if(min_x_V > bigArea_copy_V[i][j].X)
                        {
                            min_x_V = bigArea_copy_V[i][j].X;
                        }
                    }
                }
                //Debug.Log((min_x + max_x)/2);
                //openCVによって点を描画するところ
                //Cv2.Circle(mat_grid,(min_x_V + max_x_V) / 2,camHeight/2,30,new Scalar(0),10);
            }
            if (_UI)
            {
                mat_grid.DrawContours(bigArea_copy_V, -1, Scalar.Red, 2);
            }
            
            //GetComponent<RawImage>().texture = OpenCvSharp.Unity.MatToTexture(mat);

            //横
            Point[][] bigArea_copy_H = new Point[areaCount_H][];
            for (var i = 0; i < areaCount_H; i++)
            {
                bigArea_copy_H[i] = bigArea_H[i];
            }

            //中心を求めて点を打つ
            int max_x_H = 0;
            int min_x_H = camWidth;
            if (areaCount_H >= 1)
            {
                for (int i = 0; i < bigArea_copy_H.Length; i++)
                {
                    for (int j = 0; j < bigArea_copy_H[i].Length; j++)
                    {
                        //Debug.Log(bigArea_copy[i][j].X);
                        if (max_x_H < bigArea_copy_H[i][j].X)
                        {
                            max_x_H = bigArea_copy_H[i][j].X;
                        }
                        if (min_x_H > bigArea_copy_H[i][j].X)
                        {
                            min_x_H = bigArea_copy_H[i][j].X;
                        }
                    }
                }
                //Debug.Log((min_x + max_x)/2);
                //openCVによって点を描画するところ
                //Cv2.Circle(mat_grid,(min_x_H + max_x_H) / 2,camHeight/2,30,new Scalar(0),10);
            }
            if (_UI)
            {
                mat_grid.DrawContours(bigArea_copy_H, -1, Scalar.Blue, 2);

            }

            //さぼてんくんの場所指定
            //中心点
            float SaboMid_V = (min_x_V + max_x_V) / 2.0f;
            float SaboMid_H = (min_x_H + max_x_H) / 2.0f;

            //傾きを求める
            //縦
            float a_V;
            float b_V;

            a_V = (960 - SaboMid_V) / 960;
            b_V = cameraPosition_V - a_V * (-cameraPosition_V_offset);

            //横
            float a_H;
            float b_H;

            a_H = -960 / (960 - SaboMid_H);
            b_H = (1080 + cameraPosition_H_offset) - a_H * cameraPosition_H;

            //交点
            float point_x = -(b_H - b_V) / (a_H - a_V);
            float point_y = (a_H*b_V - b_H*a_V) / (a_H - a_V);

            //グリッドを作る
            int horizontalLine = 27;
            int verticalLine = 48;
            if (_grid)
            {
                //縦の線
                for (int i = 1; i <= horizontalLine; i++)
                {
                    Cv2.Line(mat_grid, 0, (1080 / horizontalLine) * i, 1920, (1080 / horizontalLine) * i, new Scalar(0), 1);
                }
                //横の線
                for (int i = 1; i <= verticalLine; i++)
                {
                    Cv2.Line(mat_grid, (1920 / verticalLine) * i, 0, (1920 / verticalLine) * i, 1080, new Scalar(0), 1);
                }

                //画角表示
                //下のカメラ
                Cv2.Line(mat_grid, cameraPosition_H, 1080 + cameraPosition_H_offset, cameraPosition_H + 1080, cameraPosition_H_offset, Scalar.Red, 3);
                Cv2.Line(mat_grid, cameraPosition_H, 1080 + cameraPosition_H_offset, cameraPosition_H - 1080, cameraPosition_H_offset, Scalar.Red, 3);
                //横のカメラ
                Cv2.Line(mat_grid, -cameraPosition_V_offset, cameraPosition_V, 1080 - cameraPosition_V_offset, cameraPosition_V + 1080, Scalar.Blue, 3);
                Cv2.Line(mat_grid, -cameraPosition_V_offset, cameraPosition_V, 1080 - cameraPosition_V_offset, cameraPosition_V - 1080, Scalar.Blue, 3);
                //カメラの位置から置かれたさぼてんくんまでの直線
                Cv2.Line(mat_grid, 0, (int)b_H, 1920, (int)(a_H * 1920 + b_H), Scalar.Green, 3);
                Cv2.Line(mat_grid, 0, (int)b_V, 1920, (int)(a_V * 1920 + b_V), Scalar.Green, 3);
                //交点に円を描画
                Cv2.Circle(mat_grid, (int)point_x, (int)point_y, 30, new Scalar(0), 10);
            }


            //テクスチャに貼り付け
            this.rawImage_V.texture = OpenCvSharp.Unity.MatToTexture(mat_V);
            this.rawImage_H.texture = OpenCvSharp.Unity.MatToTexture(mat_H);
            this.rawImage_grid.texture = OpenCvSharp.Unity.MatToTexture(mat_grid);


            //さぼてんくんの映像を映すplaneの座標を移動するためのtransformを得る
            Transform Transform_V = this.transform;
            Transform Transform_H = this.transform;

            //座標を移動する
            springSabo.transform.position = new Vector3((320.0f / 1920.0f) * point_x - 160.0f, 90.0f - ((320.0f / 1920.0f) * point_y) , -100);
            summerSabo.transform.position = new Vector3((320.0f / 1920.0f) * point_x - 160.0f, 90.0f - ((320.0f / 1920.0f) * point_y), -100);
            autumnSabo.transform.position = new Vector3((320.0f / 1920.0f) * point_x - 160.0f, 90.0f - ((320.0f / 1920.0f) * point_y), -100);
            winterSabo.transform.position = new Vector3((320.0f / 1920.0f) * point_x - 160.0f, 90.0f - ((320.0f / 1920.0f) * point_y), -100);


            //季節毎に座標を取得して移動
            /*
            if (countMov == 1)
            {
                Vector3 tmp = GameObject.Find("springSabo").transform.position;
                if (!_SaboPosition_OnceInc)
                {
                    GameObject.Find("springSabo").transform.position = new Vector3(tmp.x + SaboMid_H, tmp.y + SaboMid_V, tmp.z);
                    _SaboPosition_OnceInc = true;
                }
            }else if(countMov == 2)
            {
                Vector3 tmp = GameObject.Find("summerSabo").transform.position;
                if (!_SaboPosition_OnceInc)
                {
                    GameObject.Find("summerSabo").transform.position = new Vector3(tmp.x + SaboMid_H, tmp.y + SaboMid_V, tmp.z);
                    _SaboPosition_OnceInc = true;
                }
            }else if(countMov == 3)
            {
                Vector3 tmp = GameObject.Find("autumnSabo").transform.position;
                if (!_SaboPosition_OnceInc)
                {
                    GameObject.Find("autumnSabo").transform.position = new Vector3(tmp.x + SaboMid_H, tmp.y + SaboMid_V, tmp.z);
                    _SaboPosition_OnceInc = true;
                }
            }else if(countMov == 4)
            {
                Vector3 tmp = GameObject.Find("winterSabo").transform.position;
                if (!_SaboPosition_OnceInc)
                {
                    GameObject.Find("winterSabo").transform.position = new Vector3(tmp.x + SaboMid_H, tmp.y + SaboMid_V, tmp.z);
                    _SaboPosition_OnceInc = true;
                }
            }
            */



            //mat.DrawContours(contours, -1, Scalar.Red, 2);


            //GetComponent<RawImage>().texture = OpenCvSharp.Unity.MatToTexture(mat);

            //ビデオ流す部分

            //一定サイズを超えたというフラグがたったらここに入る
            if (_overThreshold && countMov != 5)
            {
                //冬までの動画が再生終了して5秒以上経過したらここに入る
                if (_continueJudge)
                {
                    //強制的に5秒経過した事にする
                    systemTime = 5.0;
                }
                //一定サイズを超えた瞬間からの時間を更新
                systemTime += Time.deltaTime;
                //一定サイズを超えた瞬間から5秒経過したらビデオ再生
                if (systemTime >= 5.0)
                {
                    VideoPlay();
                    _videoPlaying = true;
                    _overThreshold = false;
                }
            }
            _continueJudge = false;

            if (_videoEnd && !_overThreshold_Once)
            {
                Invoke("videoEnd_Play", 4.0f);
                _videoEnd = false;
                _videoEnd_Once = true;

                byte[] snd_data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
                snd_data[0] = 0b11011011;
                serial.Write(snd_data, 0, 1);
            }
            //*/
        }

        //var areas = 1;
        //var count = 1;

        //foreach (var contour in contours)
        //{
        //    //    //面積を算出
        //    var area = Cv2.ContourArea(contour);
        //    Debug.Log(area);

        //    //    if (count < contours.Count())
        //    //    {
        //    //        areas = areas + area ;
        //    //    }
        //    //    else
        //    //    {
        //    //        areas = areas + area;
        //    //    }
        //    //    count++;


        ////動画再生
        //var VideoPlayer = GetComponent<VideoPlayer>();
        ////VideoPlayer.Pause();
        //if (area > 100000000)
        //{
        //    VideoPlayer.Play();
        //}
        //else
        //{
        //    VideoPlayer.Pause();
        //}
        //textBox1.Text = areas;
        //}

    }

    //データ受信時
    void OnDataReceived(string message)
    {
        //Debug.Log(message);
    }
    //マウスクリック感知
    void KeybordInput()
    {
        //キーボードG押下検知
        //押した時
        if (Input.GetKeyDown(KeyCode.G)) {
            _grid = !_grid;
            //Debug.Log("keyDown g");
        }
        //離した時
        if (Input.GetKeyUp(KeyCode.G))
        {
            //Debug.Log("keyUp g");
        }
        //押してる時
        if (Input.GetKey(KeyCode.G))
        {
            //Debug.Log("pushing g");
        }
        //キーボードE押下検知
        //押した時
        if (Input.GetKeyDown(KeyCode.E))
        {
            _UI = !_UI;
        }
        if (_UI)
        {
            UICanvas.SetActive(true);
        }
        else
        {
            UICanvas.SetActive(false);
        }
        //マウスクリック検知
        if (Input.GetMouseButton(0))
        {
            MouseClick = true;
            //OP動画再生
            if(countMov == 0)
            {
                videoOp.Play();
                videoOp.loopPointReached += Endprocess;
            }
            //Debug.Log("mouse");
        }

        //byte[] snd_data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        //////snd_data[0] = 192; //0b11000000
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    _led = !_led;
        //    //snd_data[0] = 0b11000000;
        //}
        //snd_data[0] = 0b11011011;
        //else if (Input.GetKeyUp(KeyCode.R))
        //{
        //    snd_data[0] = 0b11011011;
        //}
        //
        //if (Input.GetKey(KeyCode.T))
        //{
        //    snd_data[0] |= 0b00000010;
        //}
        //else
        //{
        //    snd_data[0] &= 0b11111101;
        //}

        //if (Input.GetKey(KeyCode.Y))
        //{
        //    snd_data[0] |= 0b00000100;
        //}
        //else
        //{
        //    snd_data[0] &= 0b11111011;
        //}

        //if (Input.GetKey(KeyCode.U))
        //{
        //    snd_data[0] |= 0b00001000;
        //}
        //else
        //{
        //    snd_data[0] &= 0b11110111;
        //}

        //if (Input.GetKey(KeyCode.I))
        //{
        //    snd_data[0] |= 0b00010000;
        //}
        //else
        //{
        //    snd_data[0] &= 0b11101111;
        //}

        //if (Input.GetKey(KeyCode.O))
        //{
        //    snd_data[0] |= 0b00100000;
        //}
        //else
        //{
        //    snd_data[0] &= 0b11011111;
        //}

        //serial.Write(snd_data, 0, 1);
    }

    //動画再生
    public void VideoPlay()
    {
        byte[] snd_data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        snd_data[0] = 192; //0b11000000
        //春再生
        if (countMov == 1)
        {
            snd_data[0] = 0b11000000;
            videoSpringSabo.Play();
            videoSpring.Play();
            videoSpring.loopPointReached += Endprocess;
            springSabo.transform.position = new Vector3(springSabo.transform.position.x, springSabo.transform.position.y, 0);
        }
        //夏再生
        else if (countMov == 2)
        {
            snd_data[0] = 0b11000000;
            videoSummerSabo.Play();
            videoSummer.Play();
            videoSummer.loopPointReached += Endprocess;
            summerSabo.transform.position = new Vector3(summerSabo.transform.position.x, summerSabo.transform.position.y, 0);
        }
        //秋再生
        else if (countMov == 3)
        {
            snd_data[0] = 0b11000000;
            videoAutumnSabo.Play();
            videoAutumn.Play();
            videoAutumn.loopPointReached += Endprocess;
            autumnSabo.transform.position = new Vector3(autumnSabo.transform.position.x, autumnSabo.transform.position.y, 0);
        }
        //冬再生
        else if (countMov == 4)
        {
            snd_data[0] = 0b11000000;
            videoWinterSabo.Play();
            videoWinter.Play();
            videoWinter.loopPointReached += Endprocess;
            winterSabo.transform.position = new Vector3(winterSabo.transform.position.x, winterSabo.transform.position.y + 10, 0);
        }
        serial.Write(snd_data, 0, 1);
    }

    //動画終了処理
    public void Endprocess(VideoPlayer vp)
    {
        byte[] snd_data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        snd_data[0] = 192; //0b11000000
        //動画カウント
        if (!OnceInc || countMov==4)
        {
            countMov++;
            OnceInc = true;
        }

        //動画停止
        //Debug.Log(countMov);
        videoOp.Stop();
        videoSpring.Stop();
        videoSummer.Stop();
        videoAutumn.Stop();
        videoWinter.Stop();
        videoSpringSabo.Stop();
        videoSummerSabo.Stop();
        videoAutumnSabo.Stop();
        videoWinterSabo.Stop();

        //エンディング再生
        if (countMov == 5)
        {
            _videoEnd = true;
            _videoPlaying = false;
            Debug.Log("count 555");
            winterSabo.transform.position = new Vector3(winterSabo.transform.position.x, winterSabo.transform.position.y + 10, -100);

        }
        else
        {
            //冬までの動画が再生終了した場合フラグを立てるために処理を予約
            Invoke("videoPlaying_Down",5.0f);
        }
        snd_data[0] = 0b11011011;
        serial.Write(snd_data, 0, 1);

    }


    void videoPlaying_Down()
    {
        _videoPlaying = false;
        _continueJudge = true;
    }

    void videoEnd_Play()
    {
        videoEnd.Play();
    }
}

