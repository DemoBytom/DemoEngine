using System;
using System.Threading;

namespace Demo.Tools.Common.Extensions.LockSlim
{
    public static class ReaderWriterLockSlimExtensions
    {
        public static ReaderWriterLockSlim EnterUpgradeableReadLockBlock(
            this ReaderWriterLockSlim lockSlim,
            Action act)
        {
            lockSlim.EnterUpgradeableReadLock();
            try
            {
                act();
            }
            finally
            {
                lockSlim.ExitUpgradeableReadLock();
            }
            return lockSlim;
        }

        public static ReaderWriterLockSlim EnterWriteLockBlock(
            this ReaderWriterLockSlim lockSlim,
            Action act)
        {
            lockSlim.EnterWriteLock();
            try
            {
                act();
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
            return lockSlim;
        }

        public static ReaderWriterLockSlim EnterUpgradableReadLockBlock(
            this ReaderWriterLockSlim lockSlim,
            Action<ReaderWriterLockSlim> readAction)
        {
            lockSlim.EnterUpgradeableReadLock();
            try
            {
                readAction(lockSlim);
            }
            finally
            {
                lockSlim.ExitUpgradeableReadLock();
            }

            return lockSlim;
        }

        public static ReaderWriterLockSlim IfActionEnterWriteLockBlock(
            this ReaderWriterLockSlim lockSlim,
            Func<bool> ifFunc,
            Action writeAction)
        {
            //Check if you should enter write lock
            if (ifFunc())
            {
                _ = lockSlim.EnterWriteLockBlock(() =>
                {
                    //Check the requirement again, in rare case that value
                    //changed between if and successfully entering the lock
                    if (ifFunc())
                    {
                        writeAction();
                    }
                });
            }
            return lockSlim;
        }
    }
}