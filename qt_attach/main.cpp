#include "mainwindow.h"

#include <QApplication>
#include <QProcess>
#include <QObject>
#include <QDebug>
#include <QDir>

int main(int argc, char *argv[])
{
    QApplication a(argc, argv);

    // Set a new current working directory
    QString newDir = "C:/Program Files/Signalyst/HQPlayer 5 Desktop";
    if (QDir::setCurrent(newDir)) {
	qDebug() << "New Working Directory: " << QDir::currentPath();
    } else {
	qDebug() << "Failed to set the working directory.";
    }
    QProcess::startDetached("HQPlayer5Desktop.exe");

    MainWindow w;
    w.show();
    return a.exec();
}
