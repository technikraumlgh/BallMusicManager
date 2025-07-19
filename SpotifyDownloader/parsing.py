# run pip instsall -r requirements.txt to install dependencies
# make sure ffmpeg is installed and in your PATH

import os
import sys
import argparse
from spotipy import Spotify
from spotipy.oauth2 import SpotifyClientCredentials
from spotify_dl.spotify import fetch_tracks, parse_spotify_url
import logging
from yt_dlp import YoutubeDL
from tqdm import tqdm
from pytube import Search
import json
import hashlib
from zipfile import ZipFile
import subprocess
import datetime

SPOTIFY_CLIENT_ID = "SPOTIPY_CLIENT_ID"
SPOTIFY_CLIENT_SECRET = "SPOTIPY_CLIENT_SECRET"
scriptDir = os.path.dirname(os.path.realpath(__file__))
audiosDir = os.path.join(scriptDir, 'audios')

def getSpotifyClient() -> Spotify:
    """
    Initialize and return a Spotipy Spotify client using client credentials.
    Expects SPOTIPY_CLIENT_ID and SPOTIPY_CLIENT_SECRET in the environment.
    """
    authManager = SpotifyClientCredentials(
        client_id = SPOTIFY_CLIENT_ID,
        client_secret = SPOTIFY_CLIENT_SECRET
    )
    return Spotify(auth_manager=authManager)

def searchSong(songName: str, artist: str, outputDir=audiosDir) -> str:
    """
    Search YouTube for the given song+artist, download the highest‐bitrate audio stream,
    and return the filepath to the downloaded file.
    """

    os.makedirs(outputDir, exist_ok=True)
    
    safeName = f"{songName} - {artist}".replace("/", "_").replace("\\", "_")
    basePath = os.path.join(outputDir, generateFileNameHash(safeName).upper())
    audioPath = f"{basePath}"
    if os.path.exists(audioPath):
        return audioPath

    query = f"{songName} {artist} official audio"

    # Suppress stdout and stderr during YouTube search
    originalStdout = sys.stdout
    originalStderr = sys.stderr
    sys.stdout = open(os.devnull, 'w')
    sys.stderr = open(os.devnull, 'w')
    try:
        search = Search(query)
        if not search.results:
            # restore streams before raising exception
            sys.stdout.close()
            sys.stderr.close()
            sys.stdout = originalStdout
            sys.stderr = originalStderr
            raise RuntimeError(f"No YouTube results for {query}")
        video = search.results[0]
    finally:
        sys.stdout.close()
        sys.stderr.close()
        sys.stdout = originalStdout
        sys.stderr = originalStderr

    class SilentLogger:
        def debug(self, msg): pass
        def info(self, msg): pass
        def warning(self, msg): pass
        def error(self, msg):
            tqdm.write(f"yt_dlp error: {msg}")

    ydlOpts = {
        'format': 'bestaudio/best',
        'outtmpl': basePath,
        'postprocessors': [{
            'key': 'FFmpegExtractAudio',
            'preferredcodec': 'm4a',
            'preferredquality': '0',
        }],
        'logger': SilentLogger(),
        'no_warnings': True,
    }
    with YoutubeDL(ydlOpts) as ydl:
        ydl.download(video.watch_url)
    return audioPath


def generateJson(songsDict: dict, outputFile='song_list.json') -> None:
    """
    Generate a JSON file from the songs dictionary.
    Output format:
    [
      {
        "Path": { "type": "file", "path": "...\\somefile.m4a" },
        "Index": 0,
        "Title": "...",
        "Artist": "...",
        "Dance": "",          # you can fill this per-song if known
        "Duration": "0:03:37.808866",
        "FileHash": "ABCDEF..."
      },
      …
    ]
    """
    entries = []
    for idx, s in songsDict.items():
        path = s["audio_path"]
        fileHash = os.path.basename(path).upper()
        entries.append({
            "Path": {
                "type": "hash_embedded",
            },
            "Index": idx,
            "Title": s["name"],
            "Artist": s["artist"],
            "Dance": s.get("dance", ""),       
            "Duration": getDuration(path + ".m4a"),
            "FileHash": fileHash
        })

    with open(outputFile, 'w', encoding='utf-8') as f:
        json.dump(entries, f, indent=4)
    tqdm.write(f"Successfully wrote songs to '{outputFile}'")
    
def generateFileNameHash(fileName: str) -> str:
    """
    Generate a hash for the given filename.
    This is useful for creating unique identifiers for files.
    """
    return hashlib.sha256(fileName.encode('utf-8')).hexdigest()

def getDuration(path: str, sexagesimal: bool = True) -> str | float:
    """
    Returns the duration of the media file at `path`.
    If sexagesimal=True, returns a string 'HH:MM:SS.micro'.
    Otherwise returns a float number of seconds.
    """
    cmd = ['ffprobe',
           '-v', 'error',
           '-show_entries', 'format=duration',
           '-of', 'csv=p=0']
    if sexagesimal:
        cmd.append('-sexagesimal')
    cmd.append(path)

    # run ffprobe
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    out = result.stdout.strip()
    if not out:
        raise RuntimeError(f"ffprobe failed: {result.stderr.strip()!r}")
    
    if sexagesimal:
        return out
    else:
        return float(out)

def createManifest(song_count: int, outputFile: str = 'manifest.json') -> str:
    """
    Create a manifest.json file with version, song count, and timestamp.
    Returns the path to the manifest file.
    """
    manifest = {
        "Version": 1,
        "SongCount": song_count,
        "SavedAt": datetime.datetime.now().astimezone().isoformat()
    }
    manifest_path = os.path.join(scriptDir, outputFile)
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f)
    return manifest_path

def createPlaylist(directory: str, outputFile: str = 'playlist.zip') -> None:
    """
    Create a ZIP file containing all audio files in the specified directory.
    The ZIP file will be named 'playlist.zip' by default.
    """
    # Count audio files
    song_count = sum(
        1 for root, _, files in os.walk(directory)
        for file in files if file.endswith(('.m4a', '.mp3'))
    )
    manifest_path = createManifest(song_count)

    with ZipFile(outputFile, 'w') as zipf:
        for root, _, files in os.walk(directory):
            for file in files:
                if file.endswith(('.m4a', '.mp3')):
                    filePath = os.path.join(root, file)
                    arcname = os.path.splitext(file)[0]
                    zipf.write(filePath, arcname)
        zipf.write(os.path.join(scriptDir, 'song_list.json'), 'song_list.json')
        zipf.write(manifest_path, 'manifest.json')

    base, _ = os.path.splitext(outputFile)
    new_path = base + ".plz"
    os.rename(outputFile, new_path)
    tqdm.write(f"Created playlist ZIP: {outputFile}")


def main():
    parser = argparse.ArgumentParser(
        description="Export Spotify playlist/album/track to a dictionary of song names and artists."
    )
    parser.add_argument(
        "url",
        help="Spotify URL for a playlist, album, or track (e.g., https://open.spotify.com/playlist/...).",
    )
    args = parser.parse_args()

    # Initialize Spotify client
    sp = getSpotifyClient()

    # Parse URL and fetch tracks
    try:
        itemType, itemId = parse_spotify_url(args.url)
    except Exception as e:
        print(f"Error parsing Spotify URL: {e}", file=sys.stderr)
        sys.exit(1)

    # Suppress stdout, stderr and logging during fetch_tracks
    originalStdout = sys.stdout
    originalStderr = sys.stderr
    sys.stdout = open(os.devnull, 'w')
    sys.stderr = open(os.devnull, 'w')
    loggingDisableLevel = logging.root.manager.disable
    logging.disable(logging.CRITICAL)
    try:
        songs = fetch_tracks(sp, itemType, itemId)
    finally:
        sys.stdout.close()
        sys.stderr.close()
        sys.stdout = originalStdout
        sys.stderr = originalStderr
        logging.disable(loggingDisableLevel)

    if not songs:
        print("No songs found for the provided URL.", file=sys.stderr)
        sys.exit(1)

    # Store songs in a dictionary
    songsDict = {}
    with tqdm(total=len(songs), desc="Processing songs", unit="song") as pbar:
        for n, song in enumerate(songs):
            name = song.get("name", "")
            artist = song.get("artist", "")
            pbar.set_postfix_str(f"Downloading: {name} - {artist}")
            audioPath = searchSong(name, artist)
            songsDict[n] = {
                "name": name,
                "artist": artist,
                "audio_path": audioPath
            }
            pbar.update(1)

    # Generate JSON file before downloading songs
    generateJson(songsDict, outputFile=os.path.join(scriptDir, 'song_list.json'))

    createPlaylist(audiosDir, outputFile=os.path.join(scriptDir, 'playlist.zip'))

    tqdm.write(f"Collected {len(songsDict)} songs.")


    #return songsDict

if __name__ == "__main__":
    main()